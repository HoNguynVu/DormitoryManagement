using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.UtilityBillDTOs;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class UtilityBillService : IUtilityBillService
    {
        private readonly IUtilityBillUow _utilityBillUow;
        public UtilityBillService(IUtilityBillUow utilityBillUow)
        {
            _utilityBillUow = utilityBillUow;
        }
        public async Task<(bool Success, string Message, int StatusCode)> CreateUtilityBill(CreateBillDTO dto)
        {
            if (dto.RoomId == null)
            {
                return (false, "RoomId is required", 400);
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
                Status = "Unpaid"
            };
            _utilityBillUow.BeginTransactionAsync();
            try
            {
                _utilityBillUow.UtilityBills.Add(newBill);
                await _utilityBillUow.CommitAsync();
                return (true, "Utility bill created successfully", 201);
            }
            catch (Exception ex)
            {
                await _utilityBillUow.RollbackAsync();
                return (false, $"Failed to create utility bill: {ex.Message}", 500);
            }
        }
    }
}
