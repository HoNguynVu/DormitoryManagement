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
        Task<(bool Success, string Message, int StatusCode, IEnumerable<ManagerGetBillDTO> listBill)> GetBillsForManagerAsync(ManagerGetBillRequest request);
        Task<(bool Success, string Message, int StatusCode, Parameter para)> GetActiveParameter();
        Task<(bool Success, string Message, int StatusCode, LastMonthIndexDTO dto)> GetLastMonthIndex(RequestLastMonthIndexDTO request);
    }
}
