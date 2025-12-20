using API.Hubs;
using API.Services.Implements;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.Config;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
        policy
            .WithOrigins("http://localhost:5173") // đúng địa chỉ FE
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddDbContext<DormitoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DormitoryDB")));
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSignalR();

//Unit of Work
builder.Services.AddScoped<UnitOfWork>(sp => new UnitOfWork(sp.GetRequiredService<DormitoryDbContext>(), null));
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

builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IEmailService, EmailService>();
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
builder.Services.AddScoped<IExportService, ExportService>(); // Register IExportService
builder.Services.AddScoped<IPublicInformationService, PublicInformationService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();

// Đọc config từ appsettings.json
builder.Services.Configure<ZaloPaySettings>(builder.Configuration.GetSection("ZaloPay"));

builder.Services.AddHttpClient();
builder.Services.AddSignalR();
builder.Services.AddHostedService<API.BackgroundServices.ContractExpirationWorker>();


//Repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IViolationRepository, ViolationRepository>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

app.Run();
