using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.ContractDTOs;
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

        public async Task<(bool Success, string Message, int StatusCode, Contract? Data)> GetCurrentContractAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
                return (false, "Student ID is required.", 400, null);

            try
            {
                var student = await _uow.Students.GetByIdAsync(studentId);
                if (student == null)
                {
                    return (false, "Student not found.", 404, null);
                }

                var contract = await _uow.Contracts.GetActiveContractByStudentId(studentId);

                if (contract == null)
                {
                    return (true, "No active contract found.", 200, null);
                }
                return (true, "Success", 200, contract);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving contract: {ex.Message}", 500, null);
            }
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
                var student = await _uow.Students.GetByIdAsync(studentId);
                if (student == null)
                {
                    return (false, "Student not found.", 404);
                }
                // Check hợp đồng active
                var activeContract = await _uow.Contracts.GetActiveContractByStudentId(studentId);
                if (activeContract == null)
                {
                    return (false, "No active contract found for this student.", 404);
                }

                // Check pending request
                bool hasPending = await _uow.Contracts.HasPendingRenewalRequestAsync(studentId);
                if (hasPending)
                {
                    return (false, "A pending renewal request already exists. Please check your invoices.", 409);
                }

                // Check violations
                int violations = await _uow.Violations.CountViolationsByStudentId(studentId);
                if (violations >= 3) 
                    return (false, $"Cannot renew. Too many violations ({violations}). Contact manager.", 400);

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
                    ReceiptID = "RE-"+IdGenerator.GenerateUniqueSuffix(),
                    StudentID = studentId,
                    RelatedObjectID = activeContract.ContractID,
                    Amount = totalAmount,
                    PaymentType = "Renewal",
                    Status = "Pending",
                    PrintTime = DateTime.Now,
                    Content = $"Renewal fee for {monthsToExtend} months for contract {activeContract.ContractID}"
                };
                _uow.Receipts.Add(newReceipt);
                await _uow.CommitAsync();
                return (true, newReceipt.ReceiptID, 201);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Internal Server Error: {ex.Message}", 500);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> TerminateContractNowAsync(string studentId)
        {
            // Validation
            if (string.IsNullOrEmpty(studentId))
                return (false, "Student ID is required.", 400);

            var contract = await _uow.Contracts.GetActiveContractByStudentId(studentId);
            if (contract == null)
            {
                return (false, "No active contract found to terminate.", 404);
            }

            await _uow.BeginTransactionAsync();
            try
            {
                contract.ContractStatus = "Terminated"; 
                contract.EndDate = DateOnly.FromDateTime(DateTime.Now);  
                _uow.Contracts.Update(contract);

                // B. Trả lại slot cho phòng (Cập nhật Room)
                if (contract.Room != null)
                {
                    if (contract.Room.CurrentOccupancy > 0)
                    {
                        contract.Room.CurrentOccupancy -= 1;
                    }
                    if (contract.Room.RoomStatus == "Full")
                    {
                        contract.Room.RoomStatus = "Available";
                    }

                    _uow.Rooms.Update(contract.Room); 
                }
                await _uow.CommitAsync();
                return (true, "Contract terminated successfully due to violations.", 200);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Termination failed: {ex.Message}", 500);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> ConfirmContractExtensionAsync(string contractId, int monthsAdded)
        {
            // 1. Validation
            if (string.IsNullOrEmpty(contractId))
            {
                return (false, "Contract ID is required.", 400);
            }
            if (monthsAdded <= 0)
            {
                return (false, "Months added must be greater than 0.", 400);
            }

            await _uow.BeginTransactionAsync();
            try
            {
                // 3. Lấy thông tin hợp đồng
                var contract = await _uow.Contracts.GetByIdAsync(contractId);

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
                    if (contract.ContractStatus == "Expired")
                    {
                        contract.ContractStatus = "Active";
                    }
                }

                // 5. Cập nhật và Lưu xuống DB
                _uow.Contracts.Update(contract);
                await _uow.CommitAsync();

                return (true, $"Contract extended successfully by {monthsAdded} months. New EndDate: {contract.EndDate}", 200);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Error confirming extension: {ex.Message}", 500);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<ExpiringContractDTO>)> GetExpiringContractByManager(int daysUntilExpiration, string managerId)
        {
            // 1. Chuẩn bị ngày
            DateOnly fromDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(daysUntilExpiration));

            try
            {
                // 2. Lấy dữ liệu từ Repo
                var contracts = await _uow.Contracts.GetExpiringContractsByManagerIdAsync(fromDate, endDate, managerId);

                // 3. Mapping (Sử dụng LINQ Select)
                var dtoList = contracts.Select(c => new ExpiringContractDTO
                {
                    ContractID = c.ContractID,
                    StudentID = c.StudentID,
                    ExpirationDate = c.EndDate.HasValue ? c.EndDate.Value.ToDateTime(new TimeOnly(0, 0)) : DateTime.MinValue,
                    StudentName = c.Student != null ? c.Student.FullName : "N/A",
                    StudentEmail = c.Student != null ? c.Student.Email : "N/A",
                    RoomID = c.RoomID,
                    RoomName = c.Room != null ? c.Room.RoomName : "N/A"
                }).ToList();

                return (true, "Success", 200, dtoList);
            }
            catch (Exception ex)
            {
                // Log lỗi (Serilog...)
                return (false, $"Error retrieving expiring contracts: {ex.Message}", 500, Enumerable.Empty<ExpiringContractDTO>());
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, int numContracts)> CountExpiringContractsByManager(int daysUntilExpiration, string managerID)
        {
            DateOnly fromDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(daysUntilExpiration));
            try
            {
                int count = await _uow.Contracts.CountExpiringContractsByManagerIdAsync(fromDate, endDate, managerID);
                return (true, "Success", 200, count);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving expiring contract count: {ex.Message}", 500, 0);
            }
        }
    }
}
