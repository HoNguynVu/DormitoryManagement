using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class ContractService : IContractService
    {
        private readonly IContractUow _uow;
        public ContractService(IContractUow contractUow)
        {
            _uow = contractUow;
        }

        public async Task<string> RequestRenewalAsync(string studentId, int monthsToExtend)
        {

            // Check hợp đồng active
            var activeContract = await _uow.ContractRenewals.GetActiveContractByStudentIdAsync(studentId);
            if (activeContract == null) throw new Exception("No active contract found.");

            // Check pending invoice
            bool hasPending = await _uow.ContractRenewals.HasPendingRenewalRequestAsync(studentId);
            if (hasPending) throw new Exception("Pending renewal request exists.");

            // Check violations
            int violations = await _uow.ContractRenewals.CountViolationsByStudentAsync(studentId);
            if (violations >= 3) throw new Exception("Too many violations.");

            // 2. Tính toán
            var price  = activeContract.Room.RoomType.Price;
            decimal totalAmount = price* monthsToExtend;

            await _uow.BeginTransactionAsync();
            // 3. Tạo Entity Invoice
            var newReceipt = new Receipt
            {
                ReceiptId = IdGenerator.GenerateUniqueSuffix(),
                StudentId = studentId,
                ContractId = activeContract.ContractId, 
                Amount = totalAmount,
                PaymentType = "Renewal",
                Status = "Pending",
                PrintTime = DateTime.Now,
                Content = $"Renewal fee for {monthsToExtend} months for contract {activeContract.ContractId}"
            };

            _uow.ContractRenewals.AddRenewalReceipt(newReceipt);

            await _uow.CommitAsync();
            return newReceipt.ReceiptId;
        }

        public async Task ConfirmContractExtensionAsync(string contractId, int monthsAdded)
        {
            // 1. Lấy dữ liệu
            var contract = await _uow.ContractRenewals.GetContractByIdAsync(contractId);
            if (contract == null) throw new Exception("Contract not found.");

            // 2. Logic tính ngày kết thúc mới (Business Logic)
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);

            if (contract.EndDate.HasValue)
            {
                if (contract.EndDate.Value < today)
                {
                    contract.EndDate = today.AddMonths(monthsAdded);
                }
                else
                { 
                    contract.EndDate = contract.EndDate.Value.AddMonths(monthsAdded);
                }
            }
            else
            {
                contract.EndDate = today.AddMonths(monthsAdded);
            }

            await _uow.BeginTransactionAsync();
            // 3. Đánh dấu update (Chưa lưu)
            _uow.ContractRenewals.UpdateContract(contract);

            // 4. Commit Transaction
            await _uow.CommitAsync();
        }
    }
}
