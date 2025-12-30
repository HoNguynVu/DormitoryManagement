using BusinessObject.DTOs.PaymentDTOs;

namespace API.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForRegistration(string registrationId);
        Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForRenewal(string receiptId);
        Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForHealthInsurance(string insuranceId);
        Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForUtility(string utilityId, string accountId);
        Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForRoomChange(string receiptId);
        Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForMaintenance(string receiptId);
        Task<(int ReturnCode, string ReturnMessage)> ProcessZaloPayCallback(ZaloPayCallbackDTO cbdata);

        Task<(int StatusCode, PaymentResultDto? dto)> GetPaymentResultByAppTransId(string appTransId);
    }
}
