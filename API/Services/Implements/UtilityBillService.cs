using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.UtilityBillDTOs;
using BusinessObject.Entities;
using API.Services.Helpers;
using API.Hubs;
using Microsoft.AspNetCore.SignalR;
using API.Services.Common;

namespace API.Services.Implements
{
    public class UtilityBillService : IUtilityBillService
    {
        private readonly IUtilityBillUow _utilityBillUow;
        private readonly IHubContext<NotificationHub> _hubContext;
        public UtilityBillService(IUtilityBillUow utilityBillUow, IHubContext<NotificationHub> hubContext)
        {
            _utilityBillUow = utilityBillUow;
            _hubContext = hubContext;
        }

        private string NotiMessage(int month, int year)
        {
            return $"Your utility bill for {month}/{year} is now available. Please check your BILLS for details and make the payment on time to avoid any late fees.";
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
            var newBill = new UtilityBill
            {
                BillID = "BIL" + IdGenerator.GenerateUniqueSuffix(),
                RoomID = dto.RoomId,
                ElectricityOldIndex = lastElectricityIndex,
                ElectricityNewIndex = dto.ElectricityIndex,
                WaterOldIndex = lastWaterIndex,
                WaterNewIndex = dto.WaterIndex,
                ElectricityUsage = dto.ElectricityIndex - lastElectricityIndex,
                WaterUsage = dto.WaterIndex - lastWaterIndex,
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
                    title: "New Utility Bill Available",
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
            foreach (var noti in listNotifications)
            {
                await _hubContext.Clients.Group(noti.AccountID).SendAsync("ReceiveNotification", noti);
            }
            return (true, "Utility bill created successfully", 201);
        }

        public async Task<(bool Success, string Message, int StatusCode)> ConfirmUtilityPaymentAsync(string billId)
        {
            if (!string.IsNullOrEmpty(billId))
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
            try
            {
                _utilityBillUow.UtilityBills.Update(bill);
                await _utilityBillUow.CommitAsync();
            }
            catch (Exception ex)
            {
                return (false, $"Failed to confirm payment: {ex.Message}", 500);
            }
            return (true, "Payment confirmed successfully", 200);
        }
    }
}
