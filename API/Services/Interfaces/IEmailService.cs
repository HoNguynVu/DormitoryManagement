namespace API.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVericationEmail(string toEmail, string Otp);
        Task SendResetPasswordEmail(string toEmail, string Otp);
    }
}
