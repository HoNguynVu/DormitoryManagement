using API.Hubs;
using API.Services.Common;
using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.ConfirmDTOs;
using BusinessObject.DTOs.ContractDTOs;
using BusinessObject.DTOs.EquipmentDTO;
using BusinessObject.Entities;
using DocumentFormat.OpenXml.Office.Y2022.FeaturePropertyBag;
using Microsoft.AspNetCore.SignalR;

namespace API.Services.Implements
{
    public class ContractService : IContractService
    {
        private readonly IContractUow _uow;
        private readonly IEmailService _emailService;
        private readonly PdfService _pdfService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<IContractService> _logger;
        public ContractService(IContractUow contractUow, IEmailService emailService, IHubContext<NotificationHub> hubContext, ILogger<IContractService> logger, PdfService pdfService)
        {
            _uow = contractUow;
            _emailService = emailService;
            _hubContext = hubContext;
            _logger = logger;
            _pdfService = pdfService;
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
                    RoomName = contract.Room?.RoomName ?? "N/A", 
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

        public async Task<(bool Success, string Message, int StatusCode,string? receiptId)> RequestRenewalAsync(string studentId, int monthsToExtend)
        {

            // Validation
            if (string.IsNullOrEmpty(studentId))
                return (false, "Student ID is required.", 400,null);
            if (monthsToExtend <= 0)
                return (false, "Extension duration must be greater than 0.", 400,null);
            try
            {
                var student = await _uow.Students.GetByIdAsync(studentId);
                if (student == null)
                {
                    return (false, "Student not found.", 404,null);
                }
                // Check hợp đồng active
                var activeContract = await _uow.Contracts.GetActiveAndNearExpiringContractByStudentId(studentId);
                if (activeContract == null)
                {
                    return (false, "No active contract found for this student.", 404,null);
                }

                // Check pending request
                bool hasPending = await _uow.Contracts.HasPendingRenewalRequestAsync(studentId);
                if (hasPending)
                {
                    return (false, "A pending renewal request already exists. Please check your invoices.", 409,null);
                }

                // Check violations
                int violations = await _uow.Violations.CountViolationsByStudentId(studentId);
                if (violations >= 3)
                    return (false, $"Cannot renew. Too many violations ({violations}). Contact manager.", 400,null);

                if (activeContract.Room == null)
                    return (false, "Room data is missing, cannot calculate fee.", 422, null);
                // Calculate fee
                decimal? price = activeContract.Room?.RoomType?.Price;
                if (price == null)
                    return (false, "Room type price data is missing, cannot calculate fee.", 422, null);


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
                    Content = $"Thanh toan phi gia han hop dong {monthsToExtend} thang cho hop dong {activeContract.ContractID}"
                };
                _uow.Receipts.Add(newReceipt);
                await _uow.CommitAsync();
                return (true, newReceipt.ReceiptID, 201,newReceipt.ReceiptID);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Internal Server Error: {ex.Message}", 500, null);
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

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<SummaryContractDto> dto)> GetContractFiltered(string? keyword, string? buildingId, string? status, DateOnly? startDate,DateOnly? endDate)
        {
            var result = new List<SummaryContractDto>();
            try
            {
                var contracts = await _uow.Contracts.GetContractsFilteredAsync(keyword, buildingId, status,startDate,endDate);
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

        public async Task<(bool Success, string Message, int StatusCode, Dictionary<string, int> stat)> GetOverviewContract(string? buildingId = null)
        {
            var result = new Dictionary<string, int>();
            await _uow.BeginTransactionAsync();
            try
            {
                result = await _uow.Contracts.CountContractsByStatusAsync(buildingId);
                var total = result.Values.Sum();
                result["Total"] = total;
                result["Active"] = result.ContainsKey("Active") ? result["Active"] : 0;
                result["Expired"] = result.ContainsKey("Expired") ? result["Expired"] : 0;
                var warningCount = await _uow.Contracts.CountWarningContractsAsync(daysThreshold: 14);
                result["Warning"] = warningCount;
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
            try
            {
                // 3. Lấy thông tin hợp đồng
                var contract = await _uow.Contracts.GetDetailContractAsync(contractId);

                if (contract == null)
                {
                    return (false, "Contract not found.", 404);
                }

                DateOnly today = DateOnly.FromDateTime(DateTime.Now);

                if (contract.EndDate.HasValue && contract.EndDate.Value >= today)
                {
                    contract.EndDate = contract.EndDate.Value.AddMonths(monthsAdded);
                    contract.ContractStatus = "Active";
                }
                else
                {
                    contract.EndDate = today.AddMonths(monthsAdded);
                    contract.ContractStatus = "Active";
                }
                var account = contract.Student.Account;
                var newNoti = NotificationServiceHelpers.CreateNew(
                    accountId: account.UserId,
                    title: "Thanh toán gia hạn hợp đồng",
                    message: $"Bạn đã thanh toán thành công cho gia hạn hợp đồng. Hợp đồng mới có hiệu lực đến {contract.EndDate.Value.Day}/{contract.EndDate.Value.Month}/{contract.EndDate.Value.Year}",
                    type: "Bill"
                );

                _uow.Notifications.Add(newNoti);
                // 5. Cập nhật và Lưu xuống DB
                _uow.Contracts.Update(contract);
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
                    byte[] pdfBytes = _pdfService.GenerateExtensionContractPdf(emailDto);
                    await _emailService.SendRenewalPaymentEmailAsync(emailDto,pdfBytes);
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

        public async Task<(bool Success, string Message, int StatusCode, string? ReceiptId, string? Type)> ChangeRoomAsync(ChangeRoomRequestDto request)
        {
            // Validation
            if (request == null || string.IsNullOrEmpty(request.StudentId) || string.IsNullOrEmpty(request.NewRoomId))
            {
                return (false, "Invalid request. StudentId and NewRoomId are required.", 400, null, null);
            }

            await _uow.BeginTransactionAsync();
            try
            {
                // 1. Get active contract
                var activeContract = await _uow.Contracts.GetActiveContractByStudentId(request.StudentId);
                if (activeContract == null)
                {
                    return (false, "No active contract found for this student.", 404, null, null);
                }

                var oldRoom = activeContract.Room;
                if (oldRoom == null)
                {
                    return (false, "Current room data is missing.", 422, null, null);
                }

                // 2. Check if new room is the same as current
                if (activeContract.RoomID == request.NewRoomId)
                {
                    return (false, "New room is the same as current room.", 400, null, null);
                }

                // 3. Get new room
                var newRoom = await _uow.Rooms.GetByIdAsync(request.NewRoomId);
                if (newRoom == null)
                {
                    return (false, "New room not found.", 404, null, null);
                }

                // 4. Check new room availability
                var activeCountInNewRoom = await _uow.Contracts.CountContractsByRoomIdAndStatus(request.NewRoomId, "Active");
                if (activeCountInNewRoom >= newRoom.Capacity)
                {
                    return (false, "New room is full. Cannot change to this room.", 400, null, null);
                }

                // 5. Get old and new room prices
                decimal oldRoomPrice = oldRoom.RoomType?.Price ?? 0;
                decimal newRoomPrice = newRoom.RoomType?.Price ?? 0;

                if (oldRoomPrice == 0 || newRoomPrice == 0)
                {
                    return (false, "Room price data is missing.", 422, null, null);
                }

                // 6. Calculate remaining days in contract
                var today = DateOnly.FromDateTime(DateTime.Now);
                if (!activeContract.EndDate.HasValue)
                {
                    return (false, "This contract does not have an end date. The difference in cost cannot be calculated.", 422, null, null);
                }

                var endDate = activeContract.EndDate.Value;

                int remainingDays = (endDate.ToDateTime(new TimeOnly(0, 0)) - DateTime.Now).Days;

                if (remainingDays <= 0)
                {
                    return (false, "Your contract has expired or is due today. Please renew it before changing rooms.", 400, null, null);
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

                string? createdReceiptId = null;
                string? type = "None";
                string responseMessage = "";

                // 8. Create receipt if there's price adjustment
                if (priceAdjustment != 0)
                {
                    bool isCharge = priceAdjustment > 0;
                    type = isCharge ? "Charge" : "Refund";
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
                    if (isCharge) receipt.Content += $"CMD|{request.NewRoomId}";

                    _uow.Receipts.Add(receipt);
                    createdReceiptId = receipt.ReceiptID;

                    if (isCharge)
                    {
                        responseMessage = $" A payment receipt {createdReceiptId} has been created and is pending payment.";
                    }
                    else
                    {
                        var payment = new Payment
                        {
                            PaymentID = IdGenerator.GenerateUniqueSuffix(),
                            ReceiptID = receipt.ReceiptID,
                            Amount = Math.Abs(priceAdjustment),
                            PaymentDate = DateTime.Now,
                            PaymentMethod = "Cash", // Hoàn tiền mặt
                            Status = "Success",
                            TransactionID = "REFUND-CASH"
                        };
                        _uow.Payments.Add(payment);

                        await RoomTransactionHelper.SwapRoomLogicAsync(_uow, activeContract, request.NewRoomId);
                        responseMessage = $"Đã hoàn tiền {Math.Abs(priceAdjustment):N0} VND và đổi phòng thành công.";
                    }
                }
                else
                {
                    await RoomTransactionHelper.SwapRoomLogicAsync(_uow, activeContract, request.NewRoomId);
                    responseMessage = "Đổi phòng thành công (Không phát sinh chi phí).";
                }

                await _uow.CommitAsync();
                return (true, responseMessage, 200, createdReceiptId, type);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Internal Server Error: {ex.Message}", 500, null, null);
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

        public async Task<(bool Success, string Message, int StatusCode,PendingRequestDto? dto)> GetPendingRenewalRequestAsync(string studentId)
        {
            //validate 
            if (string.IsNullOrEmpty(studentId)) 
                return (false,"Invalid StudentId",404, null);
            try
            {
                var contract = await _uow.Contracts.GetActiveContractByStudentId(studentId);
                if (contract == null)
                    return (false,"Student doesn't have contract",400,null);
                var receipt = await _uow.Receipts.GetPendingRequestAsync(contract.ContractID);
                if (receipt == null)
                    return (false, "Student doesn't have pending request renewal", 400, null);
                var rawprice = contract.Room?.RoomType?.Price;
                int months = (receipt.Amount == rawprice) ? 12 : 6;
                var result = new PendingRequestDto
                {
                    ReceiptId = receipt.ReceiptID,
                    ReceiptDate = receipt.PrintTime,
                    Months = months,
                    TotalAmount = receipt.Amount
                };
                return (true, "Receipt data retrieved successfully.", 200, result);
            }
            catch
            {
                return (false,"Internal Server Error",500,null);
            }   
        }
        public async Task<(bool Success, string Message)> RemindBulkExpiringAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var threshold = today.AddDays(14);

            var allContracts = await _uow.Contracts.GetAllAsync();

            var targets = allContracts.Where(c =>
                (c.ContractStatus == "Active" && c.EndDate.HasValue && c.EndDate <= threshold) ||
                (c.ContractStatus == "Expired")
            ).ToList();

            if (!targets.Any()) return (true, "Không có hợp đồng nào cần nhắc nhở.");

            foreach (var contract in targets)
            {
                await SendNotificationInternalAsync(contract);
            }

            return (true, $"Đã gửi nhắc nhở thành công cho {targets.Count} sinh viên.");
        }

        public async Task<(bool Success, string Message)> RemindSingleStudentAsync(string studentId)
        {
            var contract = await _uow.Contracts.GetLastContractByStudentIdAsync(studentId);
            if (contract == null) return (false, "Không tìm thấy dữ liệu hợp đồng.");

            await SendNotificationInternalAsync(contract);
            return (true, "Đã gửi nhắc nhở thành công.");
        }

        private async Task SendNotificationInternalAsync(Contract contract)
        {
            try
            {
                var student = await _uow.Students.GetByIdAsync(contract.StudentID);

                if (student == null || string.IsNullOrEmpty(student.AccountID))
                {
                    _logger.LogWarning($"Không thể gửi thông báo. Sinh viên {contract.StudentID} chưa có tài khoản hệ thống (AccountID is null).");
                    return;
                }

                bool isExpired = contract.ContractStatus == "Expired";

                string title = isExpired
                    ? "CẢNH BÁO: Hợp đồng đã quá hạn"
                    : "Nhắc nhở: Sắp hết hạn hợp đồng";

                string dateStr = contract.EndDate.HasValue ? contract.EndDate.Value.ToString("dd/MM/yyyy") : "N/A";

                string content = isExpired
                    ? $"Hợp đồng tại phòng {contract.Room?.RoomName ?? contract.RoomID} đã hết hạn. Vui lòng liên hệ quản lý."
                    : $"Hợp đồng tại phòng {contract.Room?.RoomName ?? contract.RoomID} sắp hết hạn vào ngày {dateStr}. Vui lòng gia hạn.";

   
                // 1. Tạo Entity Notification
                var notification = NotificationServiceHelpers.CreateNew(
                    student.AccountID,
                    title,
                    content,
                    "ContractReminder"
                );

                await _uow.BeginTransactionAsync();

                _uow.Notifications.Add(notification);
                await _uow.CommitAsync();

                
                await _hubContext.Clients.User(student.AccountID)
                    .SendAsync("ReceiveNotification", notification);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                _logger.LogError(ex, $"Lỗi gửi thông báo cho SV {contract.StudentID}");
            }
        }
        
        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<ContractHistoryDto> dto)> GetContractHistoriesByStudentAsync(string accountId)
        {
            var student = await _uow.Students.GetStudentByAccountIdAsync(accountId);
            if (student == null)
            {
                return (false, "Student not found.", 404, Enumerable.Empty<ContractHistoryDto>());
            }
            var activeContract = await _uow.Contracts.GetActiveAndNearExpiringContractByStudentId(student.StudentID);
            if (activeContract == null)
            {
                return (false, "No active contract found for this student.", 404, Enumerable.Empty<ContractHistoryDto>());
            }
            var list = await _uow.Receipts.GetHistoryReceiptsAsync(PaymentConstants.TypeRenewal, activeContract.ContractID);
            var result = list.Select(r => new ContractHistoryDto
            {
                ReceiptId = r.ReceiptID,
                PrintTime = r.PrintTime,
                Content = r.Content,
                Amount = r.Amount,
                Status = r.Status
            }).ToList();
            return (true, "Success", 200, result);
        }
    }
}
