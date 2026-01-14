using API;
using API.Services.Interfaces;
using BusinessObject.DTOs.PaymentDTOs;
using DataAccess.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics; 
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Linq;

namespace API.IntegrationTests.Factories
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 1. Remove SQL Server Context cũ
                var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DormitoryDbContext>));
                if (dbDescriptor != null) services.Remove(dbDescriptor);

                // 2. Add In-Memory DB (CÓ CẤU HÌNH IGNORE TRANSACTION)
                services.AddDbContext<DormitoryDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb_Ultimate");

                    // Fix lỗi TransactionIgnoredWarning
                    options.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });

                // 3. Mock Payment Service (để test chạy qua được đoạn thanh toán)
                var paymentDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPaymentService));
                if (paymentDescriptor != null) services.Remove(paymentDescriptor);

                var mockPayment = new Mock<IPaymentService>();
                var fakeRes = new PaymentLinkDTO { IsSuccess = true, Message = "Mocked", PaymentUrl = "http://mock" };

                mockPayment.Setup(x => x.CreateZaloPayLinkForRegistration(It.IsAny<string>())).ReturnsAsync((200, fakeRes));
                // Setup thêm các hàm khác nếu cần...

                services.AddSingleton(mockPayment.Object);

                // 4. Mock Email Service (để không gửi mail thật)
                var emailDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
                if (emailDescriptor != null) services.Remove(emailDescriptor);

                var mockEmail = new Mock<IEmailService>();
                services.AddSingleton(mockEmail.Object);
            });
        }
    }
}