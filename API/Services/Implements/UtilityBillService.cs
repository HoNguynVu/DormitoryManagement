using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.UtilityBillDTOs;
using BusinessObject.Entities;
using API.Services.Helpers;
using API.Hubs;
using Microsoft.AspNetCore.SignalR;
using API.Services.Common;
using BusinessObject.DTOs.ConfirmDTOs;
using System.Collections.Immutable;

namespace API.Services.Implements
{
    public class UtilityBillService : IUtilityBillService
    {
        private readonly IUtilityBillUow _utilityBillUow;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmailService _emailService;
        private readonly ILogger<IUtilityBillService> _logger;
        public UtilityBillService(IUtilityBillUow utilityBillUow, IHubContext<NotificationHub> hubContext,IEmailService emailService,ILogger<IUtilityBillService> logger)
        {
            _utilityBillUow = utilityBillUow;
            _hubContext = hubContext;
            _emailService = emailService;
            _logger = logger;
        }

        private string NotiMessage(int month, int year)
        {
            return $"Hóa đơn điện nước {month}/{year} của bạn đã có. Hãy bấm vào mục Điện Nước để tiến hành thanh toán.";
        }

        public async Task<(bool Success, string Message, int StatusCode)> CreateUtilityBill(CreateBillDTO dto)
        {
            if (dto.RoomId == null)
            {
                return (false, "RoomId is required", 400);
            }
            bool exists = await _utilityBillUow.UtilityBills.IsBillExistsAsync(dto.RoomId, DateTime.Now.Month, DateTime.Now.Year);
            if (exists)
            {
                return (false, "Bill already exists", 400);
            }
            var lastBill = await _utilityBillUow.UtilityBills.GetLastMonthBillAsync(dto.RoomId);
            var lastElectricityIndex = lastBill?.ElectricityNewIndex ?? 0;
            var lastWaterIndex = lastBill?.WaterNewIndex ?? 0;
            if (dto.ElectricityIndex < lastElectricityIndex || dto.WaterIndex < lastWaterIndex)
            {
                return (false, "This month's index cannot be less than last month's index", 400);
            }
            var parameter = await _utilityBillUow.Parameters.GetActiveParameterAsync();
            if (parameter == null)
            {
                return (false, "Active parameter not found", 500);
            }
            var newBill = new UtilityBill
            {
                BillID = "BIL-" + IdGenerator.GenerateUniqueSuffix(),
                RoomID = dto.RoomId,
                ElectricityOldIndex = lastElectricityIndex,
                ElectricityNewIndex = dto.ElectricityIndex,
                WaterOldIndex = lastWaterIndex,
                WaterNewIndex = dto.WaterIndex,
                ElectricityUsage = dto.ElectricityIndex - lastElectricityIndex,
                WaterUsage = dto.WaterIndex - lastWaterIndex,
                ElectricityUnitPrice = parameter.DefaultElectricityPrice,
                WaterUnitPrice = parameter.DefaultWaterPrice,
                Amount = (dto.ElectricityIndex - lastElectricityIndex) * parameter.DefaultElectricityPrice +
                         (dto.WaterIndex - lastWaterIndex) * parameter.DefaultWaterPrice,
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
                Status = PaymentConstants.BillUnpaid,
            };
            var activeContracts = await _utilityBillUow.Contracts.GetContractsByRoomIdAndStatus(dto.RoomId, "Active");
            var newMessage = NotiMessage(newBill.Month, newBill.Year);
            var listNotifications = new List<Notification>();
            foreach (var contract in activeContracts)
            {
                var accountId = contract.Student.AccountID;
                var newNoti = NotificationServiceHelpers.CreateNew(
                    accountId: accountId,
                    title: "Hóa đơn điện nước mới!",
                    message: newMessage,
                    type: "UtilityBill"
                );
                listNotifications.Add(newNoti);
            }

            await _utilityBillUow.BeginTransactionAsync();
            try
            {
                _utilityBillUow.UtilityBills.Add(newBill);
                foreach (var noti in listNotifications)
                {
                    _utilityBillUow.Notifications.Add(noti);
                }
                await _utilityBillUow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _utilityBillUow.RollbackAsync();
                return (false, $"Failed to create utility bill: {ex.Message}", 500);
            }
            try
            {
                foreach (var noti in listNotifications)
                {
                    await _hubContext.Clients.User(noti.AccountID).SendAsync("ReceiveNotification", noti);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR Error: {ex.Message}");
            }
            return (true, "Utility bill created successfully", 201);
        }

        public async Task<(bool Success, string Message, int StatusCode)> ConfirmUtilityPaymentAsync(string billId)
        {
            if (string.IsNullOrEmpty(billId))
            {
                return (false, "BillId is required", 400);
            }
            var bill = await _utilityBillUow.UtilityBills.GetByIdAsync(billId);
            if (bill == null)
            {
                return (false, "Bill not found", 404);
            }

            if (bill.Status == "Paid")
            {
                return (false, "Bill is already paid", 400);
            }
            bill.Status = "Paid";
            await _utilityBillUow.BeginTransactionAsync();
            try
            {
                _utilityBillUow.UtilityBills.Update(bill);
                await _utilityBillUow.CommitAsync();
                var receipt = await _utilityBillUow.Receipts.GetReceiptByTypeAndRelatedIdAsync(PaymentConstants.TypeUtility,billId);
                if (receipt == null) 
                {
                    return (false, "Receipt not found", 404);
                }
                var student = await _utilityBillUow.Students.GetByIdAsync(receipt.Student.StudentID);
                if (student == null) {
                    return (false, "Student not found", 404);
                }
                var parameter = await _utilityBillUow.Parameters.GetActiveParameterAsync();
                if (parameter == null) {
                    return (false, "Parameter not found", 404);
                }
                var emailDto = new UtilityPaymentSuccessDto
                {
                    StudentName = student.FullName,
                    StudentEmail = student.Email,

                    ReceiptID = receipt.ReceiptID,
                    BuildingName = bill.Room.Building.BuildingName,
                    RoomName = bill.Room.RoomName,
                    BillingMonth = $"{bill.Month}/{bill.Year}",
                    PaymentDate = receipt.PrintTime,

                    // Mapping chỉ số ĐIỆN
                    ElectricIndexOld = bill.ElectricityOldIndex,
                    ElectricIndexNew = bill.ElectricityNewIndex,
                    ElectricUsage = bill.ElectricityUsage,
                    ElectricAmount = bill.ElectricityUsage*parameter.DefaultElectricityPrice,

                    // Mapping chỉ số NƯỚC
                    WaterIndexOld = bill.WaterOldIndex,
                    WaterIndexNew = bill.WaterNewIndex,
                    WaterUsage = bill.WaterUsage,
                    WaterAmount = bill.WaterUsage*parameter.DefaultWaterPrice,

                    // Tổng tiền
                    TotalAmount = bill.Amount
                };
                try
                {
                    await _emailService.SendUtilityPaymentEmailAsync(emailDto);
                }
                catch
                {
                    _logger.LogError($"Failed to send utility payment email to {student.Email} for BillID: {bill.BillID}");
                }
                return (true, "Payment confirmed successfully", 200);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to confirm payment: {ex.Message}", 500);
            }
        }
        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<UtilityBill> list)> GetBillsByAccountIdAsync(string accountId)
        {
            if (string.IsNullOrEmpty(accountId))
            {
                return (false, "AccountId is required", 400, Enumerable.Empty<UtilityBill>());
            }
            
            var account = await _utilityBillUow.Accounts.GetByIdAsync(accountId);
            if (account == null)
            {
                return (false, "Account not found", 404, Enumerable.Empty<UtilityBill>());
            }

            var student = await _utilityBillUow.Students.GetStudentByEmailAsync(account.Email);
            if (student == null)
            {
                return (false, "Student not found", 404, Enumerable.Empty<UtilityBill>());
            }

            var contract = await _utilityBillUow.Contracts.GetActiveContractByStudentId(student.StudentID);
            if (contract == null)
            {
                return (false, "No active contract found for this student", 404, Enumerable.Empty<UtilityBill>());
            }
            var billsOfRoom = await _utilityBillUow.UtilityBills.GetByRoomAsync(contract.RoomID);    

            var unpaidBills = billsOfRoom.Where(b => b.Status != PaymentConstants.BillPaid).ToList();
            return (true, "Bills retrieved successfully", 200, unpaidBills);
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<UtilityBillDetailForStudent> listBill)> GetUtilityBillsByStudent(string accountId)
        {
            if (string.IsNullOrEmpty(accountId))
            {
                return (false, "AccountId is required", 400, Enumerable.Empty<UtilityBillDetailForStudent>());
            }

            var account = await _utilityBillUow.Accounts.GetByIdAsync(accountId);
            if (account == null)
            {
                return (false, "Account not found", 404, Enumerable.Empty<UtilityBillDetailForStudent>());
            }

            var student = await _utilityBillUow.Students.GetStudentByEmailAsync(account.Email);
            if (student == null)
            {
                return (false, "Student not found", 404, Enumerable.Empty<UtilityBillDetailForStudent>());
            }

            var contract = await _utilityBillUow.Contracts.GetActiveContractByStudentId(student.StudentID);
            if (contract == null)
            {
                return (false, "No active contract found for this student", 404, Enumerable.Empty<UtilityBillDetailForStudent>());
            }
            var list = await _utilityBillUow.UtilityBills.GetByRoomAsync(contract.RoomID);

            var dtoList = new List<UtilityBillDetailForStudent>();

            foreach (var bill in list)
            {
                var dto = new UtilityBillDetailForStudent
                {
                    BillId = bill.BillID,
                    Month = bill.Month,
                    Year = bill.Year,
                    ElectricityOldIndex = bill.ElectricityOldIndex,
                    ElectricityNewIndex = bill.ElectricityNewIndex,
                    ElectricityUnitPrice = bill.ElectricityUnitPrice,
                    WaterUnitPrice = bill.WaterUnitPrice,
                    WaterOldIndex = bill.WaterOldIndex,
                    WaterNewIndex = bill.WaterNewIndex,
                    ElectricityUsage = bill.ElectricityUsage,
                    WaterUsage = bill.WaterUsage,
                    TotalAmount = bill.Amount,
                    Status = bill.Status
                };
                dtoList.Add(dto);
            }
            return (true, "Utility bills retrieved successfully", 200, dtoList);
        }
        
        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<ManagerGetBillDTO> listBill)> GetBillsForManagerAsync(ManagerGetBillRequest request)
        {
            if (string.IsNullOrEmpty(request.AccountId))
            {
                return (false, "ManagerId is required", 400, Enumerable.Empty<ManagerGetBillDTO>());
            }
            var manager = await _utilityBillUow.BuildingManagers.GetByAccountIdAsync(request.AccountId);
            if (manager == null)
            {
                return (false, "Manager not found", 404, Enumerable.Empty<ManagerGetBillDTO>());
            }

            var rooms = await _utilityBillUow.Rooms.GetRoomByManagerIdAsync(manager.ManagerID);
            if (rooms == null || !rooms.Any())
            {
                return (false, "No rooms found for this manager", 404, Enumerable.Empty<ManagerGetBillDTO>());
            }
            var dtoList = new List<ManagerGetBillDTO>();
            try
            {
                foreach (var room in rooms)
                {
                    var bill = await _utilityBillUow.UtilityBills.GetByRoomAndPeriodAsync(room.RoomID, request.Month, request.Year);
                    if (bill != null)
                    {
                        var dto = new ManagerGetBillDTO
                        {
                            RoomID = room.RoomID,
                            RoomName = room.RoomName,
                            ElectricityOldIndex = bill.ElectricityOldIndex,
                            ElectricityNewIndex = bill.ElectricityNewIndex,
                            WaterOldIndex = bill.WaterOldIndex,
                            WaterNewIndex = bill.WaterNewIndex,
                            ElectricityUsage = bill.ElectricityUsage,
                            WaterUsage = bill.WaterUsage,
                            Amount = bill.Amount,
                            Status = bill.Status
                        };
                        dtoList.Add(dto);
                    }
                    else
                    {
                        var dto = new ManagerGetBillDTO
                        {
                            RoomID = room.RoomID,
                            RoomName = room.RoomName,
                            Status = "No Bill"
                        };
                        dtoList.Add(dto);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving bills: {ex.Message}", 500, Enumerable.Empty<ManagerGetBillDTO>());
            }
            return (true, "Bills retrieved successfully", 200, dtoList);
        }
    }
}
