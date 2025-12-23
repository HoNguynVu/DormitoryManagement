using API.Services.Common;
using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.ConfirmDTOs;
using BusinessObject.DTOs.HealthInsuranceDTOs;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class HealthInsuranceService : IHealthInsuranceService
    {
        private readonly IHealthInsuranceUow _uow;
        private readonly IEmailService _emailService;
        private readonly ILogger<IHealthInsuranceService> _logger;
        public HealthInsuranceService(IHealthInsuranceUow uow, IEmailService emailService, ILogger<IHealthInsuranceService> logger)
        {
            _uow = uow;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, int StatusCode)> RegisterHealthInsuranceAsync(string studentId, string hospitalId, string cardNumber)
        {
            //Validation 
            if (string.IsNullOrEmpty(studentId))
                return (false, "Student ID is required.", 400);

            if (string.IsNullOrEmpty(hospitalId))
                return (false, "Registration place is required.", 400);

            if (!string.IsNullOrEmpty(cardNumber))
                return (false, "Card Number is required", 400);
            // Check Dkien
            var student = await _uow.Students.GetByIdAsync(studentId);
            if (student == null)
            {
                return (false, "Student not found.", 404);
            }

            var hasPending = await _uow.HealthInsurances.HasPendingInsuranceRequestAsync(studentId);

            if (hasPending)
            {
                return (false, "You already have a pending insurance request.", 409);
            }
            var currentInsurance = await _uow.HealthInsurances.GetActiveInsuranceByStudentIdAsync(studentId);
            // Kiểm tra nếu còn hạn quá dài thì không cho mua tiếp ( > 1 tháng)
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            if (currentInsurance != null && currentInsurance.EndDate > today.AddMonths(1))
            {
                return (false, $"Your current insurance is valid until {currentInsurance.EndDate}. Renewal is only allowed 1 months before expiration.", 400);
            }
            int nextYear = DateTime.Now.Year + 1;
            var healthprice = await _uow.HealthPrices.GetHealthInsuranceByYear(nextYear);
            if (healthprice == null)
            {
                return (false, "Không thể lấy giá bảo hiểm", 404);
            }
            // Add Insurance
            await _uow.BeginTransactionAsync();
            try
            {
                var healthInsurance = new HealthInsurance
                {
                    InsuranceID = "HI-" + IdGenerator.GenerateUniqueSuffix(),
                    StudentID = studentId,
                    HospitalID = hospitalId,
                    StartDate = new DateOnly(nextYear, 1, 1),
                    EndDate = new DateOnly(nextYear, 12, 31),
                    Cost = healthprice.Amount,
                    Status = "Pending",
                    CardNumber = cardNumber
                };

                _uow.HealthInsurances.Add(healthInsurance);
                await _uow.CommitAsync();
                return (true, healthInsurance.InsuranceID, 201);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"DB Error (Write): {ex.Message}", 500);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, HealthInsurance? Data)> GetInsuranceByStudentIdAsync(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                return (false, "Student ID is required.", 400, null);
            }

            try
            {
                var insurance = await _uow.HealthInsurances.GetLatestInsuranceByStudentIdAsync(studentId);

                if (insurance == null)
                {
                    return (true, "No insurance record found.", 200, null);
                }

                return (true, "Success", 200, insurance);
            }
            catch (Exception ex)
            {
                return (false, $"Database Error: {ex.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> ConfirmInsurancePaymentAsync(string insuranceId)
        {
            if (string.IsNullOrEmpty(insuranceId))
                return (false, "Insurance ID is required.", 400);

            await _uow.BeginTransactionAsync();
            try
            {
                // 1. Tìm bản ghi bảo hiểm đang chờ (Pending)
                var insurance = await _uow.HealthInsurances.GetByIdAsync(insuranceId);

                if (insurance == null)
                {
                    return (false, "Health insurance record not found.", 404);
                }

                if (insurance.Status == "Active")
                {
                    return (true, "Insurance is already active.", 200);
                }

                // 3. Cập nhật thông tin bản ghi
                insurance.Status = "Active";

                _uow.HealthInsurances.Update(insurance);
                // 4. Lưu và Commit
                await _uow.CommitAsync();

                var emailDto = new HealthInsurancePurchaseDto
                {
                    StudentName = insurance.Student.FullName,
                    StudentEmail = insurance.Student.Email,
                    InsurancePeriod = $"{insurance.StartDate.Year} - {insurance.EndDate.Year}",
                    CoverageStartDate = insurance.StartDate,
                    CoverageEndDate = insurance.EndDate,
                    Cost = insurance.Cost
                };
                try
                {
                    await _emailService.SendInsurancePaymentEmailAsync(emailDto);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError($"Failed to send insurance confirmation email: {emailEx.Message}");
                }
                return (true, $"Insurance activated successfully. Valid from {insurance.StartDate} to {insurance.EndDate}", 200);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Error activating insurance: {ex.Message}", 500);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> CreateHealthInsurancePrice(CreateHealthPriceDTO request)
        {
            //validate
            if (request == null)
                return (false, "Không có dữ liệu đầu vào", 400);
            if (request.Amount < 0)
                return (false, " Gía tiền BHYT không được âm", 400);
            await _uow.BeginTransactionAsync();
            try
            {

                var oldhealthprice = await _uow.HealthPrices.GetHealthInsuranceByYear(request.Year);
                if (oldhealthprice != null)
                {
                    oldhealthprice.IsActive = false;
                }
                var newPrice = new HealthInsurancePrice
                {
                    HealthPriceID = "HP-" + IdGenerator.GenerateUniqueSuffix(),
                    Amount = request.Amount,
                    Year = request.Year,
                    EffectiveDate = request.EffectiveDate,
                    IsActive = true
                };

                // 3. Lưu vào DB
                _uow.HealthPrices.Add(newPrice);
                await _uow.CommitAsync();

                return (true, "Tạo mới giá BHYT thành công.", 201); // 201 Created
            }
            catch (Exception ex)
            {

                return (false, $"Lỗi hệ thống: {ex.Message}", 500);
            }
        }
    }
}
