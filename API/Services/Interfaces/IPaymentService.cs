using BusinessObject.DTOs.PaymentDTOs;

namespace API.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<(int StatusCode, PaymentLinkDTO dto)> CreateZaloPayLinkForRegistration(string registrationId);
        Task<(int ReturnCode, string ReturnMessage)> ProcessZaloPayCallback(ZaloPayCallbackDTO cbdata);
    }
}
