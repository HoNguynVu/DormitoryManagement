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
            var student = await _uow.Students.GetStudentByIdAsync(studentId);
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

            // Add Receipt
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

                var receipt = new Receipt
                {
                    ReceiptID = IdGenerator.GenerateUniqueSuffix(),
                    StudentID = studentId,
                    Amount = Cost.INSURANCE_COST_PER_YEAR,
                    PaymentType = "HealthInsurance",
                    RelatedObjectID = healthInsurance.InsuranceID,
                    Status = "Pending",
                    PrintTime = DateTime.Now,
                    Content = $"Health Insurance Fee (Place: {registrationPlace})"
                };

                _uow.Receipts.AddReceipt(receipt);
                _uow.HealthInsurances.Add(healthInsurance);
                await _uow.CommitAsync();

                return (true, receipt.ReceiptID, 201);
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
    }
}
