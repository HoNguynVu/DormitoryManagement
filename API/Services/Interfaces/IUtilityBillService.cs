using API.UnitOfWorks;
using BusinessObject.DTOs.UtilityBillDTOs;
using BusinessObject.Entities;


namespace API.Services.Interfaces
{
    public interface IUtilityBillService
    {
        Task<(bool Success, string Message, int StatusCode, IEnumerable<UtilityBillDetailForStudent> listBill)> GetUtilityBillsByStudent(string accountId);
        Task<(bool Success, string Message, int StatusCode)> CreateUtilityBill(CreateBillDTO dto);
        Task<(bool Success, string Message, int StatusCode)> ConfirmUtilityPaymentAsync(string billId);
    }
}
