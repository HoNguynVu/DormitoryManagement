using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Google.Apis.Auth.OAuth2;
using API.Services.Interfaces;
namespace API.Services.Implements
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration configuration)
        {
            _config = configuration;
        }
        public async Task SendVericationEmail(string toEmail, string Otp)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("NoReply", _config["Email:From"]));
            emailMessage.To.Add(MailboxAddress.Parse(toEmail));
            emailMessage.Subject = "Email Verification Otp";
            emailMessage.Body = new TextPart("plain")
            {
                Text = $"Your verification OTP is: {Otp}"
            };
            await SendEmailAsync(emailMessage);
        }

        private async Task SendEmailAsync(MimeMessage emailMessage)
        {
            var credential = GoogleCredential
                .FromJsonParameters(new JsonCredentialParameters
                {
                    ClientId = _config["Email:ClientId"],
                    ClientSecret = _config["Email:ClientSecret"],
                    RefreshToken = _config["Email:RefreshToken"],
                    Type = "authorized_user"
                })
                .CreateScoped("https://mail.google.com/");
            var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(new SaslMechanismOAuth2(_config["Email:From"], accessToken));
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}
