using API.Services.Interfaces;
using BusinessObject.DTOs.ConfirmDTOs;
using Google.Apis.Auth.OAuth2;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Globalization;
using System.Net.Mail;
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

        public async Task SendResetPasswordEmail(string toEmail, string Otp)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("NoReply", _config["Email:From"]));
            emailMessage.To.Add(MailboxAddress.Parse(toEmail));
            emailMessage.Subject = "Reset Password Otp";
            emailMessage.Body = new TextPart("plain")
            {
                Text = $"Your verification OTP is: {Otp}"
            };
            await SendEmailAsync(emailMessage);
        }

        public async Task SendRegistrationPaymentEmailAsync(DormRegistrationSuccessDto dto)
        {
            var culture = new CultureInfo("vi-VN");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("NoReply", _config["Email:From"]));
            message.To.Add(new MailboxAddress(dto.StudentName, dto.StudentEmail));
            message.Subject = $"[KTX] Xác nhận thanh toán phí đăng ký phòng {dto.RoomName}";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; color: #333;'>
                <h2 style='color: #0056b3;'>XÁC NHẬN THANH TOÁN ĐĂNG KÝ PHÒNG</h2>
                <p>Chào bạn <strong>{dto.StudentName}</strong>,</p>
                <p>Chúng tôi đã nhận được khoản thanh toán phí đăng ký của bạn. Chúc mừng bạn đã chính thức trở thành cư dân của KTX.</p>
            
                <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
                    <tr style='background-color: #f2f2f2;'><td style='padding: 10px;'>Mã hợp đồng:</td><td style='padding: 10px;'><strong>{dto.ContractCode}</strong></td></tr>
                    <tr><td style='padding: 10px;'>Phòng/Tòa:</td><td style='padding: 10px;'>{dto.RoomName} - {dto.BuildingName}</td></tr>
                    <tr style='background-color: #f2f2f2;'><td style='padding: 10px;'>Loại phòng:</td><td style='padding: 10px;'>{dto.RoomType}</td></tr>
                    <tr><td style='padding: 10px;'>Thời gian bắt đầu:</td><td style='padding: 10px;'>{dto.StartDate:dd/MM/yyyy}</td></tr>
                    <tr style='background-color: #e8f4fd;'><td style='padding: 10px; font-weight: bold;'>Số tiền đã đóng:</td><td style='padding: 10px; font-weight: bold; color: #d9534f;'>{dto.DepositAmount.ToString("N0", culture)} VNĐ</td></tr>
                </table>

                <p>Vui lòng mang theo CCCD và email này đến văn phòng quản lý để nhận chìa khóa khi đến nhận phòng.</p>
                <p>Trân trọng,<br>Ban Quản lý KTX.</p>
            </div>";

            message.Body = bodyBuilder.ToMessageBody();
            await SendEmailAsync(message);
        }

        public async Task SendRenewalPaymentEmailAsync(DormRenewalSuccessDto dto)
        {
            var culture = new CultureInfo("vi-VN");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("NoReply", _config["Email:From"]));
            message.To.Add(new MailboxAddress(dto.StudentName, dto.StudentEmail));
            message.Subject = $"[KTX] Biên lai thu phí gia hạn hợp đồng {dto.ContractCode}";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; color: #333;'>
                <h2 style='color: #28a745;'>GIA HẠN THÀNH CÔNG</h2>
                <p>Chào bạn <strong>{dto.StudentName}</strong>,</p>
                <p>Giao dịch thanh toán phí lưu trú cho kỳ tiếp theo của bạn đã thành công.</p>
            
                <div style='border: 1px dashed #ccc; padding: 15px; background-color: #fafafa;'>
                    <p><strong>Thông tin gia hạn:</strong></p>
                    <ul>
                        <li>Mã hợp đồng: {dto.ContractCode}</li>
                        <li>Phòng hiện tại: {dto.RoomName} ({dto.BuildingName})</li>
                        <li>Thời gian gia hạn: <strong>{dto.NewStartDate:dd/MM/yyyy}</strong> đến <strong>{dto.NewEndDate:dd/MM/yyyy}</strong></li>
                        <li>Tổng tiền thanh toán: <span style='color: #d9534f; font-weight: bold; font-size: 16px;'>{dto.TotalAmountPaid.ToString("N0", culture)} VNĐ</span></li>
                    </ul>
                </div>
            
                <p>Hợp đồng của bạn đã được cập nhật trên hệ thống.</p>
                <p>Trân trọng,<br>Ban Quản lý KTX.</p>
            </div>";

            message.Body = bodyBuilder.ToMessageBody();
            await SendEmailAsync(message);
        }

        public async Task SendInsurancePaymentEmailAsync(HealthInsurancePurchaseDto dto)
        {
            var culture = new CultureInfo("vi-VN");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("NoReply", _config["Email:From"]));
            message.To.Add(new MailboxAddress(dto.StudentName, dto.StudentEmail));
            message.Subject = $"[BHYT] Xác nhận đăng ký và thanh toán BHYT năm {dto.Year}";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; color: #333;'>
                <h2 style='color: #17a2b8;'>XÁC NHẬN THANH TOÁN BHYT</h2>
                <p>Chào bạn <strong>{dto.StudentName}</strong>,</p>
                <p>Bạn đã đăng ký và thanh toán thành công Bảo hiểm Y tế cho năm <strong>{dto.Year}</strong>.</p>
            
                <table style='width: 100%; border: 1px solid #ddd; margin-bottom: 20px;'>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'>Hiệu lực từ:</td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{dto.CoverageStartDate:dd/MM/yyyy}</td></tr>
                    <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'>Đến ngày:</td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{dto.CoverageEndDate:dd/MM/yyyy}</td></tr>
                    <tr><td style='padding: 8px; font-weight: bold;'>Số tiền:</td><td style='padding: 8px; font-weight: bold; color: #d9534f;'>{dto.Cost.ToString("N0", culture)} VNĐ</td></tr>
                </table>

                <div style='background-color: #fff3cd; padding: 10px; color: #856404; border-radius: 4px;'>
                </div>
                <p>Trân trọng,<br>Phòng Công tác Sinh viên & KTX.</p>
            </div>";

            message.Body = bodyBuilder.ToMessageBody();
            await SendEmailAsync(message);
        }

        public async Task SendUtilityPaymentEmailAsync(UtilityPaymentSuccessDto dto)
        {
            var culture = new CultureInfo("vi-VN");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("NoReply", _config["Email:From"]));
            message.To.Add(new MailboxAddress(dto.StudentName, dto.StudentEmail));
            message.Subject = $"[Hóa Đơn] Xác nhận thanh toán điện nước tháng {dto.BillingMonth}";

            var bodyBuilder = new BodyBuilder();
            // HTML tạo bảng chi tiết giống hóa đơn
            bodyBuilder.HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: auto; border: 1px solid #eee; padding: 20px;'>
                <div style='text-align: center; border-bottom: 2px solid #0056b3; padding-bottom: 10px; margin-bottom: 20px;'>
                    <h2 style='margin: 0; color: #0056b3;'>BIÊN LAI ĐIỆN NƯỚC</h2>
                    <p style='margin: 5px 0;'>Tháng: {dto.BillingMonth}</p>
                </div>

                <p><strong>Phòng:</strong> {dto.RoomName} - {dto.BuildingName}</p>
                <p><strong>Người thanh toán:</strong> {dto.StudentName}</p>
                <p><strong>Ngày thanh toán:</strong> {dto.PaymentDate:dd/MM/yyyy HH:mm}</p>
                <p><strong>Mã hóa đơn:</strong> {dto.ReceiptID}</p>

                <table style='width: 100%; border-collapse: collapse; margin-top: 20px; font-size: 14px;'>
                    <thead style='background-color: #f8f9fa;'>
                        <tr>
                            <th style='border: 1px solid #ddd; padding: 8px; text-align: left;'>Dịch vụ</th>
                            <th style='border: 1px solid #ddd; padding: 8px; text-align: center;'>Chỉ số đầu</th>
                            <th style='border: 1px solid #ddd; padding: 8px; text-align: center;'>Chỉ số cuối</th>
                            <th style='border: 1px solid #ddd; padding: 8px; text-align: center;'>Sử dụng</th>
                            <th style='border: 1px solid #ddd; padding: 8px; text-align: right;'>Thành tiền</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td style='border: 1px solid #ddd; padding: 8px;'>Điện</td>
                            <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{dto.ElectricIndexOld}</td>
                            <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{dto.ElectricIndexNew}</td>
                            <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{dto.ElectricUsage}</td>
                            <td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>{dto.ElectricAmount.ToString("N0", culture)}</td>
                        </tr>
                        <tr>
                            <td style='border: 1px solid #ddd; padding: 8px;'>Nước</td>
                            <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{dto.WaterIndexOld}</td>
                            <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{dto.WaterIndexNew}</td>
                            <td style='border: 1px solid #ddd; padding: 8px; text-align: center;'>{dto.WaterUsage}</td>
                            <td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>{dto.WaterAmount.ToString("N0", culture)}</td>
                        </tr>
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan='4' style='border: 1px solid #ddd; padding: 8px; text-align: right; font-weight: bold;'>TỔNG CỘNG</td>
                            <td style='border: 1px solid #ddd; padding: 8px; text-align: right; font-weight: bold; color: #d9534f; font-size: 16px;'>
                                {dto.TotalAmount.ToString("N0", culture)} VNĐ
                            </td>
                        </tr>
                    </tfoot>
                </table>

                <p style='margin-top: 30px; font-style: italic; font-size: 12px; text-align: center; color: #777;'>
                    Đây là email tự động, vui lòng không trả lời.<br>Ban Quản lý KTX.
                </p>
            </div>";

            message.Body = bodyBuilder.ToMessageBody();
            await SendEmailAsync(message);
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
            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(new SaslMechanismOAuth2(_config["Email:From"], accessToken));
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}
