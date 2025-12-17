using Microsoft.AspNetCore.SignalR;
using API.Hubs;
using API.Services.Interfaces;
using BusinessObject.Entities; // Để dùng class Notification
using DataAccess.Interfaces;
using API.UnitOfWorks;

namespace API.BackgroundServices
{
    public class ContractExpirationWorker : BackgroundService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ContractExpirationWorker> _logger;

        public ContractExpirationWorker(
            IHubContext<NotificationHub> hubContext,
            IServiceScopeFactory scopeFactory,
            ILogger<ContractExpirationWorker> logger)
        {
            _hubContext = hubContext;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker check hợp đồng hết hạn đang chạy...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckForExpiringContracts(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xảy ra trong quá trình kiểm tra hợp đồng.");
                }

                // Chờ 24 giờ
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task CheckForExpiringContracts(CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {   
                var _contractUow = scope.ServiceProvider.GetRequiredService<IContractUow>();
                var contractService = scope.ServiceProvider.GetRequiredService<IContractService>();

                var managers = await _contractUow.BuildingManagers.GetAllAsync();

                foreach (var manager in managers)
                {
                    // Logic: Kiểm tra 30 ngày
                    var result = await contractService.CountExpiringContractsByManager(30, manager.ManagerID);
                    var account = await _contractUow.Accounts.GetAccountByManagerId(manager.ManagerID);

                    if (result.numContracts > 0)
                    {
                        string message = $"Bạn có {result.numContracts} hợp đồng sắp hết hạn trong vòng 30 ngày tới.";

                        var targetUserId = account.UserId;

                        if (!string.IsNullOrEmpty(targetUserId))
                        {
                            // 4. Gửi SignalR (Dùng Clients.User thay vì Group nếu chưa cấu hình Group)
                            await _hubContext.Clients.User(targetUserId).SendAsync("ReceiveNotification", new
                            {
                                Title = "Hợp đồng sắp hết hạn",
                                Message = message,
                                Type = "ContractExpiration"
                            }, cancellationToken: stoppingToken);

                            var noti = new Notification
                            {
                                NotificationID = Guid.NewGuid().ToString(),
                                AccountID = targetUserId,
                                Title = "Hợp đồng sắp hết hạn",
                                Message = message,
                                Type = "ContractExpiration",
                                IsRead = false,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _contractUow.BeginTransactionAsync();
                            try
                            {
                                _contractUow.Notifications.Add(noti);
                                await _contractUow.CommitAsync();
                            }
                            catch (Exception ex)
                            {
                                await _contractUow.RollbackAsync();
                                _logger.LogError(ex, "Lỗi khi lưu Notification vào Database.");
                            }
                        }
                    }
                }
            }
        }
    }
}