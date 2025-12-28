using API.UnitOfWorks;

namespace API.BackgroundServices
{
    public class ContractStatusWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ContractStatusWorker> _logger;

        public ContractStatusWorker(IServiceScopeFactory scopeFactory, ILogger<ContractStatusWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker: ContractStatusWorker đang khởi động...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessContractUpdates(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xảy ra trong quá trình cập nhật trạng thái hợp đồng.");
                }

                // Cấu hình thời gian chạy: Ví dụ 12 tiếng quét 1 lần
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        private async Task ProcessContractUpdates(CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var contractUow = scope.ServiceProvider.GetRequiredService<IContractUow>();

                // Lấy ngày hiện tại (Chỉ lấy phần ngày, bỏ phần giờ phút giây để so sánh chính xác)
                var today = DateOnly.FromDateTime(DateTime.Now); 
                var thirtyDaysLater = today.AddDays(30);

                // 1. Lấy tất cả hợp đồng chưa bị hủy (Cancelled) hoặc đã thanh lý (Liquidated)
                // Chỉ quan tâm những hợp đồng đang Active hoặc đã NearExpiration (để check xem nó Expired chưa)
                var activeContracts = await contractUow.Contracts.GetAllAsync();

                var contractsToProcess = activeContracts.Where(c =>
                    c.ContractStatus == "Active" ||
                    c.ContractStatus == "NearExpiration"
                ).ToList();

                int updatedCount = 0;

                foreach (var contract in contractsToProcess)
                {
                    bool isChanged = false;
                    var endDate = contract.EndDate;

                    if (endDate < today)
                    {
                        if (contract.ContractID != "Expired")
                        {
                            contract.ContractStatus = "Expired";
                            isChanged = true;
                        }
                    }
                    // CASE 2: Sắp hết hạn (Ngày kết thúc nằm trong khoảng từ hôm nay đến 30 ngày tới)
                    else if (endDate >= today && endDate <= thirtyDaysLater)
                    {
                        if (contract.ContractStatus != "NearExpiration")
                        {
                            contract.ContractStatus = "NearExpiration";
                            isChanged = true;
                        }
                    }
                    // CASE 3: Nếu gia hạn thành công (EndDate > 30 ngày) mà status vẫn là NearExpiration thì đổi lại Active
                    else if (endDate > thirtyDaysLater)
                    {
                        if (contract.ContractStatus == "NearExpiration")
                        {
                            contract.ContractStatus = "Active";
                            isChanged = true;
                        }
                    }

                    if (isChanged)
                    {
                        await contractUow.BeginTransactionAsync();
                        contractUow.Contracts.Update(contract);
                        await contractUow.CommitAsync();
                        updatedCount++;
                    }
                }

                if (updatedCount > 0)
                {
                    await contractUow.CommitAsync();
                    _logger.LogInformation($"Worker: Đã cập nhật trạng thái mới cho {updatedCount} hợp đồng.");
                }
                else
                {
                    _logger.LogInformation("Worker: Không có hợp đồng nào cần cập nhật trạng thái.");
                }
            }
        }
    }
}
