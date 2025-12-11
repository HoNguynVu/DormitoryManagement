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

        public async Task<(bool Success, string Message, int StatusCode)> RequestRenewalAsync(string studentId, int monthsToExtend)
        {
           
             // Validation
             if (string.IsNullOrEmpty(studentId))
                return (false, "Student ID is required.", 400);
             if (monthsToExtend <= 0)
                return (false, "Extension duration must be greater than 0.", 400);
            try
            {
                // Check hợp đồng active
                var activeContract = await _uow.ContractRenewals.GetActiveContractByStudentIdAsync(studentId);
                if (activeContract == null)
                {
                    return (false, "No active contract found for this student.", 404);
                }

                // Check pending request
                bool hasPending = await _uow.ContractRenewals.HasPendingRenewalRequestAsync(studentId);
                if (hasPending)
                {
                    return (false, "A pending renewal request already exists. Please check your invoices.", 409);
                }

                // Check violations
                int violations = await _uow.ContractRenewals.CountViolationsByStudentAsync(studentId);
                if (violations >= 3) return (false, $"Cannot renew. Too many violations ({violations}). Contact manager.", 400);

                if (activeContract.Room == null)
                    return (false, "Room data is missing, cannot calculate fee.", 422);
                // Calculate fee
                decimal? price = activeContract.Room?.RoomType?.Price;
                if (price == null)
                    return (false, "Room type price data is missing, cannot calculate fee.", 422);
                decimal totalAmount = price.Value * (decimal)monthsToExtend;

                await _uow.BeginTransactionAsync();

                // Add receipt
                var newReceipt = new Receipt
                {
                    ReceiptId = IdGenerator.GenerateUniqueSuffix(),
                    StudentId = studentId,
                    RelatedObjectId = activeContract.ContractId,
                    ReceiptID = IdGenerator.GenerateUniqueSuffix(),
                    StudentID = studentId,
                    RelatedObjectID = activeContract.ContractID,
                    Amount = totalAmount,
                    PaymentType = "Renewal",
                    Status = "Pending",
                    PrintTime = DateTime.Now,
                    Content = $"Renewal fee for {monthsToExtend} months for contract {activeContract.ContractID}"
                };
                _uow.ContractRenewals.AddRenewalReceipt(newReceipt);
                await _uow.CommitAsync();
                return (true, newReceipt.ReceiptID, 201);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Internal Server Error: {ex.Message}", 500);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> ConfirmContractExtensionAsync(string contractId, int monthsAdded)
        {
            //Validation
            if (string.IsNullOrWhiteSpace(contractId))
            {
                return (false, "Contract ID is required.", 400);
            }

            if (monthsAdded <= 0)
            {
                return (false, "Months added must be a positive integer.", 400);
            }

            try
            {
                var contract = await _uow.ContractRenewals.GetContractByIdAsync(contractId);
                if (contract == null)
                {
                    return (false, "Contract not found.", 404);
                }
                DateOnly today = DateOnly.FromDateTime(DateTime.Now);
                if (contract.EndDate.HasValue && contract.EndDate.Value >= today)
                {
                    contract.EndDate = contract.EndDate.Value.AddMonths(monthsAdded);
                }
                else
                {
                    contract.EndDate = today.AddMonths(monthsAdded);
                }
                await _uow.BeginTransactionAsync();
                _uow.ContractRenewals.UpdateContract(contract);
                await _uow.CommitAsync();
                return (true, "Contract extended successfully.", 200);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Internal Server Error: {ex.Message}", 500);
            }

        }
    }
}
