namespace API.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVericationEmail(string toEmail, string Otp);
    }
}
