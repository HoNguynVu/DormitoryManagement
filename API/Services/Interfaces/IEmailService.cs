using BusinessObject.DTOs.ConfirmDTOs;

namespace API.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVericationEmail(string toEmail, string Otp);
        Task SendResetPasswordEmail(string toEmail, string Otp);
        Task SendRegistrationPaymentEmailAsync(DormRegistrationSuccessDto dto);
        Task SendRenewalPaymentEmailAsync(DormRenewalSuccessDto dto);
        Task SendTerminatedNotiToStudentAsync(DormTerminationDto dto);
        Task SendInsurancePaymentEmailAsync(HealthInsurancePurchaseDto dto);
        Task SendUtilityPaymentEmailAsync(UtilityPaymentSuccessDto dto);

    }
}
