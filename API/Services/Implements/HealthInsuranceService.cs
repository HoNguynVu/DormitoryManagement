using API.Services.Common;
using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class HealthInsuranceService : IHealthInsuranceService
    {
        private readonly IHealthInsuranceUow _uow;
        public HealthInsuranceService(IHealthInsuranceUow uow)
        {
            _uow = uow;
        }

        public async Task<(bool Success, string Message, int StatusCode)> RegisterHealthInsuranceAsync(string studentId, string registrationPlace)
        {
            //Validation 
            if (string.IsNullOrEmpty(studentId))
                return (false, "Student ID is required.", 400);
            if (string.IsNullOrEmpty(registrationPlace))
                return (false, "Registration place is required.", 400);
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
            // Kiểm tra nếu còn hạn quá dài thì không cho mua tiếp ( > 3 tháng)
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            if (currentInsurance != null && currentInsurance.EndDate > today.AddMonths(3))
            {
                return (false, $"Your current insurance is valid until {currentInsurance.EndDate}. Renewal is only allowed 3 months before expiration.", 400);
            }

            // Add Insurance
            await _uow.BeginTransactionAsync();
            try
            {
                var healthInsurance = new HealthInsurance
                {
                    InsuranceID = "HI-" + IdGenerator.GenerateUniqueSuffix(),
                    StudentID = studentId,
                    InitialHospital = registrationPlace,
                    StartDate = DateOnly.FromDateTime(DateTime.Now),
                    EndDate = DateOnly.FromDateTime(DateTime.Now),
                    Cost = Cost.INSURANCE_COST_PER_YEAR,
                    Status = "Pending",
                    CardNumber = ""
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

                // 2. Logic tính ngày hiệu lực 
                DateOnly today = DateOnly.FromDateTime(DateTime.Now);
                DateOnly startDate = today;

                // Kiểm tra xem sinh viên có bảo hiểm cũ nào đang Active không 
                var activeOldInsurance = await _uow.HealthInsurances.GetActiveInsuranceByStudentIdAsync(insurance.StudentID);

                if (activeOldInsurance != null && activeOldInsurance.EndDate >= today)
                {
                    // Trường hợp A: Có bảo hiểm cũ còn hạn -> Nối tiếp
                    startDate = activeOldInsurance.EndDate.AddDays(1);
                }
                else
                {
                    // Trường hợp B: Không có hoặc đã hết hạn -> Tính từ hôm nay
                    startDate = today;
                }

                // 3. Cập nhật thông tin bản ghi
                insurance.Status = "Active";
                insurance.StartDate = startDate;
                insurance.EndDate = startDate.AddYears(1); // Mặc định mua 1 năm
                insurance.CardNumber = IdGenerator.GenerateUniqueSuffix(); 

                _uow.HealthInsurances.Update(insurance);
                // 4. Lưu và Commit
                await _uow.CommitAsync();
                return (true, $"Insurance activated successfully. Valid from {insurance.StartDate} to {insurance.EndDate}", 200);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Error activating insurance: {ex.Message}", 500);
            }
        }
    }
}
