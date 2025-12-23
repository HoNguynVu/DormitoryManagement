using API.Services.Common;
using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.ConfirmDTOs;
using BusinessObject.DTOs.ContractDTOs;
using BusinessObject.DTOs.EquipmentDTO;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class ContractService : IContractService
    {
        private readonly IContractUow _uow;
        private readonly IEmailService _emailService;
        private readonly ILogger<IContractService> _logger;
        public ContractService(IContractUow contractUow, IEmailService emailService, ILogger<IContractService> logger)
        {
            _uow = contractUow;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, int StatusCode, ContractDto? Data)> GetCurrentContractAsync(string studentId)
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
                if (contract== null)
                    return (false,"No active contract found",200,null);

                var contractDto = new ContractDto
                {
                    ContractId = contract.ContractID,
                    RoomName = contract.Room?.RoomName ?? "N/A", // Null check an toàn
                    StudentName = contract.Student?.FullName ?? "N/A",
                    StartDate = contract.StartDate,
                    EndDate = contract.EndDate,
                    Status = contract.ContractStatus,
                };
                return (true, "Success", 200, contractDto);
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


                decimal totalAmount = (monthsToExtend == 12) ? price.Value : price.Value * 0.5m; // giá phòng theo năm

                await _uow.BeginTransactionAsync();

                // Add receipt
                var newReceipt = new Receipt
                {
                    ReceiptID = "RE-" + IdGenerator.GenerateUniqueSuffix(),
                    StudentID = studentId,
                    RelatedObjectID = activeContract.ContractID,
                    Amount = totalAmount,
                    PaymentType = PaymentConstants.TypeRenewal,
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

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<SummaryContractDto> dto)> GetContractFiltered(string? keyword, string? buildingName, string? status)
        {
            var result = new List<SummaryContractDto>();
            await _uow.BeginTransactionAsync();
            try
            {
                var contracts = await _uow.Contracts.GetContractsFilteredAsync(keyword, buildingName, status);
                result = contracts.Select(c => new SummaryContractDto
                {
                    ContractID = c.ContractID,
                    StudentID = c.StudentID,
                    StudentName = c.Student != null ? c.Student.FullName : "N/A",
                    RemainingDays = c.EndDate.HasValue ? (c.EndDate.Value.ToDateTime(new TimeOnly(0, 0)) - DateTime.Now).Days : 0,
                    RoomName = c.Room != null ? c.Room.RoomName : "N/A",
                    BuildingName = c.Room != null && c.Room.Building != null ? c.Room.Building.BuildingName : "N/A",
                    StartDate = c.StartDate,
                    EndDate = c.EndDate.HasValue ? c.EndDate.Value : DateOnly.MinValue,
                    Status = c.ContractStatus
                }).ToList();
                return (true, "Success", 200, result);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving contracts: {ex.Message}", 500, Enumerable.Empty<SummaryContractDto>());
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, Dictionary<string, int> stat)> GetOverviewContract()
        {
            var result = new Dictionary<string, int>();
            await _uow.BeginTransactionAsync();
            try
            {
                result = await _uow.Contracts.CountContractsByStatusAsync();
                var total = result.Values.Sum();
                result["Total"] = total;
                result["Active"] = result.ContainsKey("Active") ? result["Active"] : 0;
                result["Expired"] = result.ContainsKey("Expired") ? result["Expired"] : 0;
                return (true, "Success", 200, result);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving contract statistics: {ex.Message}", 500, new Dictionary<string, int>());
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> RejectRenewalAsync(RejectRenewalDto dto)
        {
            // 1. Validate Input
            if (string.IsNullOrEmpty(dto.ReceiptId))
                return (false, "Receipt ID is required.", 400);

            // 2. Tìm hóa đơn
            var receipt = await _uow.Receipts.GetByIdAsync(dto.ReceiptId);
            if (receipt == null)
                return (false, $"Renewal request {dto.ReceiptId}  not found.", 404);

            // 3. Kiểm tra tính hợp lệ
            // Chỉ được từ chối các hóa đơn đang chờ (Pending)
            if (receipt.Status != "Pending")
            {
                return (false, $"Cannot reject. Current status is {receipt.Status}.", 400);
            }

            // Chỉ được từ chối đúng loại hóa đơn Gia hạn (Renewal)
            if (receipt.PaymentType != PaymentConstants.TypeRenewal)
            {
                return (false, "This receipt is not a renewal request.", 400);
            }

            // 4. Xử lý từ chối
            receipt.Status = "Cancelled"; // Hoặc "Rejected" tùy enum của bạn

            // Ghi chú lý do vào nội dung hóa đơn để lưu vết
            receipt.Content += $" | Rejected by Manager. Reason: {dto.Reason}";

            // await _notificationService.SendAsync(receipt.StudentID, "Yêu cầu gia hạn bị từ chối: " + dto.Reason);

            // 5. Lưu xuống DB
            await _uow.BeginTransactionAsync();
            _uow.Receipts.Update(receipt);
            await _uow.CommitAsync();

            return (true, "Renewal request rejected successfully.", 200);
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
                // 6 . Gửi email xác nhận 

                var receipt = await _uow.Receipts.GetReceiptByTypeAndRelatedIdAsync(PaymentConstants.TypeRenewal, contract.ContractID);
                if (receipt == null)
                {
                    return (false, "Associated receipt not found.", 404);
                }
                var emailDto = new DormRenewalSuccessDto
                {
                    StudentEmail = contract.Student?.Email ?? "N/A",
                    StudentName = contract.Student?.FullName ?? "N/A",
                    ContractCode = contract.ContractID,
                    BuildingName = contract.Room?.Building?.BuildingName ?? "N/A",
                    RoomName = contract.Room?.RoomName ?? "N/A",
                    OldEndDate = contract.EndDate.HasValue ? contract.EndDate.Value.AddMonths(-monthsAdded) : DateOnly.MinValue,
                    NewStartDate = contract.EndDate.HasValue ? contract.EndDate.Value.AddMonths(-monthsAdded).AddDays(1) : DateOnly.MinValue,
                    NewEndDate = contract.EndDate ?? DateOnly.MinValue,
                    TotalAmountPaid = receipt.Amount
                };
                try
                {
                    await _emailService.SendRenewalPaymentEmailAsync(emailDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi gửi mail BHYT cho SV {contract.Student?.StudentID}");
                }
                return (true, $"Contract extended successfully by {monthsAdded} months. New EndDate: {contract.EndDate}", 200);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Error confirming extension: {ex.Message}", 500);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, DetailContractDto dto)> GetDetailContract(string contractId)
        {
            if (string.IsNullOrEmpty(contractId))
            {
                return (false, "Contract ID is required.", 400, new DetailContractDto());
            }
            var result = new DetailContractDto();
            try
            {
                var contract = await _uow.Contracts.GetDetailContractAsync(contractId);
                if (contract == null)
                {
                    return (false, "Contract not found.", 404, result);
                }
                var daysremaining = contract.EndDate.HasValue ? (contract.EndDate.Value.ToDateTime(new TimeOnly(0, 0)) - DateTime.Now).Days : 0;
                result = new DetailContractDto
                {
                    ContractID = contract.ContractID,
                    Status = contract.ContractStatus,
                    DaysRemaining = daysremaining,
                    StartDate = contract.StartDate,
                    EndDate = contract.EndDate.HasValue ? contract.EndDate.Value : DateOnly.MinValue,

                    StudentID = contract.StudentID,
                    StudentName = contract.Student != null ? contract.Student.FullName : "N/A",
                    StudentPhone = contract.Student != null ? contract.Student.PhoneNumber : "N/A",
                    StudentEmail = contract.Student != null ? contract.Student.Email : "N/A",

                    RoomName = contract.Room != null ? contract.Room.RoomName : "N/A",
                    BuildingName = contract.Room != null && contract.Room.Building != null ? contract.Room.Building.BuildingName : "N/A",
                    RoomTypeName = contract.Room != null && contract.Room.RoomType != null ? contract.Room.RoomType.TypeName : "N/A",
                    MaxCapacity = contract.Room != null && contract.Room.RoomType != null ? contract.Room.RoomType.Capacity : 0,
                    RoomPrice = contract.Room != null && contract.Room.RoomType != null ? contract.Room.RoomType.Price : 0
                };
                return (true, "Success", 200, result);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving contract details: {ex.Message}", 500, result);
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

        public async Task<(bool Success, string Message, int StatusCode)> ChangeRoomAsync(ChangeRoomRequestDto request)
        {
            // Validation
            if (request == null || string.IsNullOrEmpty(request.StudentId) || string.IsNullOrEmpty(request.NewRoomId))
            {
                return (false, "Invalid request. StudentId and NewRoomId are required.", 400);
            }

            await _uow.BeginTransactionAsync();
            try
            {
                // 1. Get active contract
                var activeContract = await _uow.Contracts.GetActiveContractByStudentId(request.StudentId);
                if (activeContract == null)
                {
                    return (false, "No active contract found for this student.", 404);
                }

                var oldRoom = activeContract.Room;
                if (oldRoom == null)
                {
                    return (false, "Current room data is missing.", 422);
                }

                // 2. Check if new room is the same as current
                if (activeContract.RoomID == request.NewRoomId)
                {
                    return (false, "New room is the same as current room.", 400);
                }

                // 3. Get new room
                var newRoom = await _uow.Rooms.GetByIdAsync(request.NewRoomId);
                if (newRoom == null)
                {
                    return (false, "New room not found.", 404);
                }

                // 4. Check new room availability
                var activeCountInNewRoom = await _uow.Contracts.CountContractsByRoomIdAndStatus(request.NewRoomId, "Active");
                if (activeCountInNewRoom >= newRoom.Capacity)
                {
                    return (false, "New room is full. Cannot change to this room.", 400);
                }

                // 5. Get old and new room prices
                decimal oldRoomPrice = oldRoom.RoomType?.Price ?? 0;
                decimal newRoomPrice = newRoom.RoomType?.Price ?? 0;

                if (oldRoomPrice == 0 || newRoomPrice == 0)
                {
                    return (false, "Room price data is missing.", 422);
                }

                // 6. Calculate remaining days in contract
                var today = DateOnly.FromDateTime(DateTime.Now);
                var endDate = activeContract.EndDate ?? today.AddMonths(6); // default 6 months if no end date
                var remainingDays = endDate.DayNumber - today.DayNumber;

                if (remainingDays <= 0)
                {
                    return (false, "Contract has expired or expiring today. Cannot change room.", 400);
                }

                // 7. Calculate price adjustment based on reason and price difference
                decimal priceAdjustment = 0;
                string receiptContent = "";

                // Calculate price difference (prices are yearly)
                decimal monthlyDifference = newRoomPrice - oldRoomPrice;
                decimal dailyOldPrice = oldRoomPrice / 365; // Yearly price divided by 365 days
                decimal dailyNewPrice = newRoomPrice / 365; // Yearly price divided by 365 days
                decimal remainingAmount = remainingDays * (dailyNewPrice - dailyOldPrice);

                if (request.Reason == ChangeRoomReasonEnum.DormitoryIssue)
                {
                    // If due to dormitory issues, reduce old room fee by 50% for remaining days
                    decimal refund = (remainingDays * dailyOldPrice) * 0.5m;

                    if (monthlyDifference > 0)
                    {
                        // Upgrading to higher price room
                        priceAdjustment = remainingAmount - refund;
                        receiptContent = $"Đổi phòng do sự cố KTX từ {oldRoom.RoomName} sang {newRoom.RoomName}. " +
                                       $"Hoàn {refund:N0} VND (50% phí {remainingDays} ngày còn lại phòng cũ). " +
                                       $"Thu thêm {remainingAmount:N0} VND cho phòng mới.";
                    }
                    else if (monthlyDifference < 0)
                    {
                        // Downgrading to lower price room
                        priceAdjustment = -refund; // Only refund 50% of old room, no additional charge
                        receiptContent = $"Đổi phòng do sự cố KTX từ {oldRoom.RoomName} sang {newRoom.RoomName}. " +
                                       $"Hoàn {refund:N0} VND (50% phí {remainingDays} ngày còn lại phòng cũ).";
                    }
                    else
                    {
                        // Same price
                        priceAdjustment = -refund;
                        receiptContent = $"Đổi phòng do sự cố KTX từ {oldRoom.RoomName} sang {newRoom.RoomName}. " +
                                       $"Hoàn {refund:N0} VND (50% phí {remainingDays} ngày còn lại).";
                    }
                }
                else
                {
                    // Personal request or other reasons
                    if (monthlyDifference > 0)
                    {
                        // Upgrading - must pay difference for remaining days
                        priceAdjustment = remainingAmount;
                        receiptContent = $"Đổi phòng theo yêu cầu từ {oldRoom.RoomName} sang {newRoom.RoomName}. " +
                                       $"Thu thêm {priceAdjustment:N0} VND cho {remainingDays} ngày còn lại.";
                    }
                    else if (monthlyDifference < 0)
                    {
                        // Downgrading - no refund
                        priceAdjustment = 0;
                        receiptContent = $"Đổi phòng theo yêu cầu từ {oldRoom.RoomName} sang {newRoom.RoomName}. " +
                                       $"Không hoàn phí khi đổi xuống phòng giá thấp hơn.";
                    }
                    else
                    {
                        // Same price
                        priceAdjustment = 0;
                        receiptContent = $"Đổi phòng theo yêu cầu từ {oldRoom.RoomName} sang {newRoom.RoomName}. " +
                                       $"Cùng mức giá, không phát sinh thêm phí.";
                    }
                }


                // 8. Create receipt if there's price adjustment
                if (priceAdjustment != 0)
                {
                    var receipt = new Receipt
                    {
                        ReceiptID = "RE-" + IdGenerator.GenerateUniqueSuffix(),
                        StudentID = request.StudentId,
                        RelatedObjectID = activeContract.ContractID,
                        Amount = Math.Abs(priceAdjustment),
                        PaymentType = priceAdjustment > 0 ? "RoomChangeCharge" : "RoomChangeRefund",
                        Status = priceAdjustment > 0 ? "Pending" : "Completed",
                        PrintTime = DateTime.Now,
                        Content = receiptContent + (string.IsNullOrEmpty(request.ManagerNote) ? "" : $" Ghi chú: {request.ManagerNote}")
                    };

                    _uow.Receipts.Add(receipt);
                }

                await _uow.CommitAsync();

                string responseMessage = priceAdjustment > 0
                    ? $"Yêu cầu đổi phòng đã được chấp nhận. Sinh viên cần thanh toán thêm {priceAdjustment:N0} VND."
                    : priceAdjustment < 0
                        ? $"Yêu cầu đổi phòng đã được chấp nhận. Hoàn tiền {Math.Abs(priceAdjustment):N0} VND cho sinh viên."
                        : "Yêu cầu đổi phòng đã được chấp nhận";

                return (true, responseMessage, 200);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Internal Server Error: {ex.Message}", 500);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> ConfirmRefundAsync(ConfirmRefundDto request)
        {
            // Validation
            if (request == null || string.IsNullOrEmpty(request.ReceiptId))
            {
                return (false, "Invalid request. ReceiptId is required.", 400);
            }

            if (string.IsNullOrEmpty(request.RefundMethod))
            {
                return (false, "RefundMethod is required (BankTransfer, Cash, etc.).", 400);
            }

            await _uow.BeginTransactionAsync();
            try
            {
                // 1. Get receipt
                var receipt = await _uow.Receipts.GetByIdAsync(request.ReceiptId);
                if (receipt == null)
                {
                    return (false, "Receipt not found.", 404);
                }

                // 2. Validate receipt type
                if (receipt.PaymentType != "RoomChangeRefund")
                {
                    return (false, "This receipt is not a refund receipt.", 400);
                }

                // 3.
                var student = await _uow.Students.GetByIdAsync(receipt.StudentID);
                if (student == null)
                {
                    return (false, "Student not found.", 404);
                }

                var activeContract = await _uow.Contracts.GetActiveContractByStudentId(student.StudentID);
                if (activeContract == null)
                {
                    return (false, "Associated contract not found.", 404);
                }
                // 8. Update contract
                activeContract.RoomID = request.ReceiptId;
                _uow.Contracts.Update(activeContract);
                var oldRoom = await _uow.Rooms.GetByIdAsync(activeContract.RoomID);
                if (oldRoom == null)
                {
                    return (false, "Old room data is missing.", 422);
                }
                var newRoom = await _uow.Rooms.GetByIdAsync(request.NewRoomId);
                if (newRoom == null)
                {
                    return (false, "New room not found.", 404);
                }
                // 9. Update old room occupancy
                if (oldRoom.CurrentOccupancy > 0)
                {
                    oldRoom.CurrentOccupancy -= 1;
                }
                if (oldRoom.CurrentOccupancy < oldRoom.Capacity && oldRoom.RoomStatus == "Full")
                {
                    oldRoom.RoomStatus = "Available";
                }
                _uow.Rooms.Update(oldRoom);

                //10.Update new room occupancy
                newRoom.CurrentOccupancy += 1;
                if (newRoom.CurrentOccupancy >= newRoom.Capacity)
                {
                    newRoom.RoomStatus = "Full";
                }
                _uow.Rooms.Update(newRoom);

                // Update content with refund confirmation info
                var confirmationNote = $" | Đã hoàn tiền qua {request.RefundMethod}";
                if (!string.IsNullOrEmpty(request.TransactionReference))
                {
                    confirmationNote += $" - Mã GD: {request.TransactionReference}";
                }
                if (!string.IsNullOrEmpty(request.ManagerNote))
                {
                    confirmationNote += $" - Ghi chú: {request.ManagerNote}";
                }
                confirmationNote += $" - Xác nhận lúc: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                receipt.Content += confirmationNote;

                _uow.Receipts.Update(receipt);

                await _uow.CommitAsync();

                return (true, $"Đã xác nhận hoàn tiền {receipt.Amount:N0} VND cho sinh viên {receipt.StudentID}.", 200);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Internal Server Error: {ex.Message}", 500);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, ContractDetailByStudentDto? dto)> GetContractDetailByStudentAsync(string accountId)
        {
            try
            {
                // 1. Get Student
                var student = await _uow.Students.GetStudentByAccountIdAsync(accountId);
                if (student == null)
                {
                    return (false, "Student not found.", 404, null); // Trả về null thay vì new Dto()
                }

                // 2. Get Contract (Đảm bảo Repo đã Include Room, Building, Manager, Equipment)
                var contract = await _uow.Contracts.GetLastContractByStudentIdAsync(student.StudentID);
                if (contract == null)
                {
                    return (false, "No contract found for this student.", 404, null);
                }


                var dto = new ContractDetailByStudentDto
                {
                    ContractID = contract.ContractID,
                    Status = contract.ContractStatus,
                    StartDate = contract.StartDate,
                    EndDate = contract.EndDate ?? DateOnly.MinValue,

                    ManagerID = contract.Room?.Building?.ManagerID ?? "N/A",
                    ManagerName = contract.Room?.Building?.Manager?.FullName ?? "N/A",
                    ManagerEmail = contract.Room?.Building?.Manager?.Email ?? "N/A",
                    ManagerPhone = contract.Room?.Building?.Manager?.PhoneNumber ?? "N/A",

                    RoomName = contract.Room?.RoomName ?? "N/A",
                    BuildingName = contract.Room?.Building?.BuildingName ?? "N/A",
                    RoomTypeName = contract.Room?.RoomType?.TypeName ?? "N/A",
                    RoomPrice = contract.Room?.RoomType?.Price ?? 0,

                    Equipments = contract.Room?.RoomEquipments.Select(re => new EquipmentOfRoomDTO
                    {
                        EquipmentID = re.EquipmentID,
                        EquipmentName = re.Equipment.EquipmentName,
                        Quantity = re.Quantity,
                        Status = re.Status,
                    }).ToList() ?? new List<EquipmentOfRoomDTO>()
                };

                return (true, "Success", 200, dto);
            }
            catch (Exception ex)
            {
                // TODO: Inject ILogger và log ex.Message ở đây (Server log)
                // Console.WriteLine(ex.ToString()); 

                return (false, $"Error retrieving contract details: {ex.Message}", 500, null);
            }
        }
    }
}
