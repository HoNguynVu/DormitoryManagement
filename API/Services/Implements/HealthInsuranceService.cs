using API.Services.Common;
using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.ConfirmDTOs;
using BusinessObject.DTOs.ContractDTOs;
using BusinessObject.DTOs.HealthInsuranceDTOs;
using BusinessObject.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics.Contracts;

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

        public async Task<(bool Success, string Message, int StatusCode,string? insuranceId)> RegisterHealthInsuranceAsync(string studentId, string hospitalId, string cardNumber)
        {
            //Validation 
            if (string.IsNullOrEmpty(studentId))
                return (false, "Student ID is required.", 400, null);

            if (string.IsNullOrEmpty(hospitalId))
                return (false, "Registration place is required.", 400, null);

            if (string.IsNullOrEmpty(cardNumber))
                return (false, "Card Number is required", 400, null);
            // Check Dkien
            var student = await _uow.Students.GetByIdAsync(studentId);
            if (student == null)
            {
                return (false, "Student not found.", 404, null);
            }

            var hasPending = await _uow.HealthInsurances.HasPendingInsuranceRequestAsync(studentId);

            if (hasPending)
            {
                return (false, "You already have a pending insurance request.", 409, null);
            }
            var currentInsurance = await _uow.HealthInsurances.GetActiveInsuranceByStudentIdAsync(studentId);
            // Kiểm tra nếu còn hạn quá dài thì không cho mua tiếp ( > 1 tháng)
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            if (currentInsurance != null && currentInsurance.EndDate > today.AddMonths(1))
            {
                return (false, $"Your current insurance is valid until {currentInsurance.EndDate}. Renewal is only allowed 1 months before expiration.", 400,null);
            }
            int nextYear = DateTime.Now.Year + 1;
            var healthprice = await _uow.HealthPrices.GetHealthInsuranceByYear(nextYear);
            if (healthprice == null)
            {
                return (false, "Không thể lấy giá bảo hiểm", 404, null);
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
                    CardNumber = cardNumber,
                    HealthPriceID = healthprice.HealthPriceID
                };

                _uow.HealthInsurances.Add(healthInsurance);
                await _uow.CommitAsync();
                return (true, healthInsurance.InsuranceID, 201,healthInsurance.InsuranceID);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"DB Error (Write): {ex.InnerException?.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, SummaryHealthDto? dto)> GetInsuranceByStudentIdAsync(string studentId)
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

                var result = new SummaryHealthDto
                {
                    HealthInsuranceId = insurance.InsuranceID,
                    StudentName = insurance.Student.FullName,
                    Status = insurance.Status,
                    CardNumber = insurance.CardNumber,
                    HospitalName = insurance.Hospital.HospitalName
                };

                return (true, "Success", 200, result);
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
            try
            {
                // 1. Tìm bản ghi bảo hiểm đang chờ (Pending)
                var insurance = await _uow.HealthInsurances.GetDetailInsuranceByIdAsync(insuranceId);

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
                var account = insurance.Student.Account;
                var newNoti = NotificationServiceHelpers.CreateNew(
                    accountId: account.UserId,
                    title: "Thanh toán bảo hiểm y tế",
                    message: $"Bạn đã thanh toán thành công cho bảo hiểm y tế năm {insurance.StartDate.Year}",
                    type: "Bill"
                );

                _uow.Notifications.Add(newNoti);
                _uow.HealthInsurances.Update(insurance);

                var emailDto = new HealthInsurancePurchaseDto
                {
                    StudentName = insurance.Student.FullName,
                    StudentEmail = insurance.Student.Email,
                    Year = insurance.StartDate.Year,
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

        public async Task<(bool Success, string Message, int StatusCode,string? healthPriceId)> CreateHealthInsurancePriceAsync(CreateHealthPriceDTO request)
        {
            //validate
            if (request == null)
                return (false, "Không có dữ liệu đầu vào", 400, null);
            if (request.Amount < 0)
                return (false, " Giá tiền BHYT không được âm", 400, null);
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

                return (true, "Tạo mới giá BHYT thành công.", 201,newPrice.HealthPriceID); // 201 Created
            }
            catch (Exception ex)
            {

                return (false, $"Lỗi hệ thống: {ex.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, HealthDetailDto dto)> GetDetailHealth(string insuranceId)
        {
            // validate
            if (string.IsNullOrWhiteSpace(insuranceId))
                return (false, "Invalid InsuranceId", 400, new HealthDetailDto());

            try
            {
                var result = await _uow.HealthInsurances.GetDetailInsuranceByIdAsync(insuranceId);
                if (result == null)
                    return (false, "Insurance Not Founded", 404, new HealthDetailDto());
                var detailDto = new HealthDetailDto
                {
                    HealthInsuranceId = result.InsuranceID,
                    StudentName = result.Student.FullName,
                    Status = result.Status,
                    CardNumber = result.CardNumber,
                    HospitalName = result.Hospital.HospitalName,
                    StartDate = result.StartDate,
                    EndDate = result.EndDate,
                    Price = result.Cost,
                    Email = result.Student.Email
                };
                return (true, "Lấy dữ liệu thành công", 200, detailDto);
            }
            catch
            {
                return (false,"Internal Server Error",500, new HealthDetailDto());
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<SummaryHealthDto> dto)> GetHealthInsuranceFiltered(string? keyword, string? hospitalName, int? year, string? status)
        {
            try
            {
                var list = await _uow.HealthInsurances.GetHealthInsuranceFiltered(keyword, hospitalName, year, status);
                var result = list.Select(h => new SummaryHealthDto
                {
                    HealthInsuranceId = h.InsuranceID,
                    StudentName = h.Student.FullName,
                    Status = h.Status,
                    CardNumber = h.CardNumber,
                    HospitalName = h.Hospital.HospitalName
                }).ToList();
                return (true,"Lấy dữ liệu thành công",200,result);
            }
            catch
            {
                return (false, "Lỗi server", 500, Enumerable.Empty<SummaryHealthDto>());
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, HealthPriceDto? dto)> GetHealthPriceByYear(int year)
        {
            try
            {
                var price = await _uow.HealthPrices.GetHealthInsuranceByYear(year);
                if (price == null)
                    return (false, "Error get price by year", 400, null);
                var result = new HealthPriceDto
                {
                    HealthPriceId = price.HealthPriceID,
                    Price = price.Amount,
                    IsActive = price.IsActive,
                    Year = price.Year,
                };
                return (true, "Health insurance price data get successful", 200, result);
            }
            catch
            {
                return(false,"Internal Server Error",500,null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<SummaryHospitalDto> listHospital)> GetAllHospitalAsync()
        {
            try
            {
                var result = await _uow.HealthInsurances.GetAllHospitalAsync();
                var list = result.Select(h => new SummaryHospitalDto
                {
                    HospitalId = h.HospitalID,
                    HospitalName = h.HospitalName,
                }).ToList();
                return (true, "Get Hospital Successfully", 200, list);
            }
            catch
            {
                return (false,"Internal Server Error",500,Enumerable.Empty<SummaryHospitalDto>());
            }
        }
    }
}
