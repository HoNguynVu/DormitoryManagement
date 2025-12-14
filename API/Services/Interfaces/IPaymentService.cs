using BusinessObject.DTOs.PaymentDTOs;

namespace API.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForRegistration(string registrationId);
        Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForRenewal(string receiptId);
        Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForHealthInsurance(string insuranceId);
        Task<(int ReturnCode, string ReturnMessage)> ProcessZaloPayCallback(ZaloPayCallbackDTO cbdata);
    }
}
