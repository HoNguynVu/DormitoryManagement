using API.BackgroundServices; // Thêm namespace này
using API.Hubs;
using API.Services.Implements;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.Config; // Namespace chứa class ZaloPaySettings
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
        policy
            .WithOrigins("http://localhost:5173") // Địa chỉ React
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// 2. Load Configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 3. Database Context
builder.Services.AddDbContext<DormitoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DormitoryDB")));

// 4. Core Services
builder.Services.AddControllers();
builder.Services.AddSignalR(); // Chỉ gọi 1 lần ở đây
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5. Unit of Work
builder.Services.AddScoped<UnitOfWork>(sp => new UnitOfWork(sp.GetRequiredService<DormitoryDbContext>(), null));
// Map các Interface UoW vào Implementation cụ thể
builder.Services.AddScoped<IAuthUow>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IRegistrationUow>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IContractUow>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IRoomUow>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IViolationUow>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IPaymentUow>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IHealthInsuranceUow>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IMaintenanceUow>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IUtilityBillUow>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IBuildingUow>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IPublicInformationUow>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IParameterUow>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IStudentUow>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IRoomTypeUow>(sp => sp.GetRequiredService<UnitOfWork>());

// 6. Business Services
builder.Services.AddScoped<IEmailService, EmailService>(); // Sửa lại: Không cần AddScoped<EmailService> riêng
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IViolationService, ViolationService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IHealthInsuranceService, HealthInsuranceService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<IUtilityBillService, UtilityBillService>();
builder.Services.AddScoped<IBuildingManagerService, BuildingManagerService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IPublicInformationService, PublicInformationService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();

// 7. Repositories (Nếu UoW đã bao gồm Repo thì có thể không cần dòng này, nhưng giữ lại nếu code cũ cần)
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IViolationRepository, ViolationRepository>();

// 8. Background Services
builder.Services.AddHostedService<ContractExpirationWorker>();

// ============================================================
// 9. CẤU HÌNH ZALOPAY (SỬA LỖI TẠI ĐÂY)
// ============================================================
builder.Services.Configure<ZaloPaySettings>(builder.Configuration.GetSection("ZaloPaySettings"));

// Đăng ký IOptions<ZaloPaySettings>



// 10. Authentication (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"] ?? string.Empty)
            )
        };
    });

builder.Services.Configure<ZaloPaySettings>(builder.Configuration.GetSection("ZaloPaySettings"));

builder.Services.AddSingleton(VnPaySettings =>
{
    var config = builder.Configuration.GetSection("ZaloPaySettings");
    return new ZaloPaySettings
    {
        AppId = config["AppId"] ?? string.Empty,
        Key1 = config["Key1"] ?? string.Empty,
        Key2 = config["Key2"] ?? string.Empty,
        CreateOrderUrl = config["CreateOrderUrl"] ?? string.Empty,
        CallbackUrl = config["CallbackUrl"] ?? string.Empty,
        FrontEndUrl = config["FrontEndUrl"] ?? string.Empty
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// UseCors phải đặt trước UseAuthorization
app.UseCors("AllowReactApp");

app.UseAuthentication(); // Thêm dòng này để kích hoạt JWT
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();