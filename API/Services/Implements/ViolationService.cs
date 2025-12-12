using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.ViolationDTOs;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class ViolationService : IViolationService
    {
        private readonly IViolationUow _violationUow;
        private const int MAX_VIOLATIONS_BEFORE_TERMINATION = 3;

        public ViolationService(IViolationUow violationUow)
        {
            _violationUow = violationUow;
        }

        public async Task<(bool Success, string Message, int StatusCode, ViolationResponse? Data)> CreateViolationAsync(CreateViolationRequest request)
        {
            try
            {
                // ===== TRANSACTION 1: CREATE VIOLATION =====
                await _violationUow.BeginTransactionAsync();
                
                var newViolation = new Violation
                {
                    ViolationID = "VL-" + IdGenerator.GenerateUniqueSuffix(),
                    StudentID = request.StudentId,
                    ReportingManagerID = request.BuildingManagerId,
                    ViolationAct = request.ViolationAct,
                    Description = request.Description,
                    ViolationTime = DateTime.UtcNow,
                    Resolution = null
                };

                _violationUow.Violations.Add(newViolation);
                await _violationUow.CommitAsync(); // Commit violation
                
                // ===== QUERY: COUNT VIOLATIONS (no transaction needed) =====
                var totalViolations = await _violationUow.Violations.CountViolationsByStudentId(request.StudentId);
                
                // ===== TRANSACTION 2: TERMINATE CONTRACT (if needed) =====
                if (totalViolations >= MAX_VIOLATIONS_BEFORE_TERMINATION)
                {
                    await _violationUow.BeginTransactionAsync(); // New transaction
                    
                    var activeContract = await _violationUow.Contracts.GetActiveContractByStudentId(request.StudentId);
                    if (activeContract != null)
                    {
                        activeContract.ContractStatus = "Terminated";
                        activeContract.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
                        _violationUow.Contracts.Update(activeContract);
                        
                        await _violationUow.CommitAsync(); // Commit contract update
                    }
                }

                // ===== RETURN RESPONSE =====
                var createdViolation = await _violationUow.Violations.GetByIdAsync(newViolation.ViolationID);
                if (createdViolation == null)
                {
                    return (false, "Failed to retrieve created violation.", 500, null);
                }

                var response = MapToResponse(createdViolation, totalViolations);

                string message = totalViolations >= MAX_VIOLATIONS_BEFORE_TERMINATION
                    ? "Violation created. Student has 3 violations, contract terminated."
                    : "Violation created successfully.";

                return (true, message, 201, response);
            }
            catch (Exception ex)
            {
                await _violationUow.RollbackAsync();
                return (false, $"Error creating violation: {ex.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> UpdateViolationAsync(UpdateViolationRequest request)
        {
            await _violationUow.BeginTransactionAsync();
            try
            {
                var violation = await _violationUow.Violations.GetByIdAsync(request.ViolationId);
                if (violation == null)
                {
                    return (false, "Violation not found.", 404);
                }

                violation.Resolution = request.Resolution;
                _violationUow.Violations.Update(violation);

                await _violationUow.CommitAsync();

                return (true, "Resolution updated successfully.", 200);
            }
            catch (Exception ex)
            {
                await _violationUow.RollbackAsync();
                return (false, $"Error updating resolution: {ex.Message}", 500);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetViolationsByStudentIdAsync(string studentId)
        {
            try
            {
                var violations = await _violationUow.Violations.GetViolationsByStudentId(studentId);
                var totalCount = violations.Count();

                var response = violations.Select(v => MapToResponse(v, totalCount));

                return (true, "Violations retrieved successfully.", 200, response);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving violations: {ex.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetAllViolationsAsync()
        {
            try
            {
                var violations = await _violationUow.Violations.GetAllAsync();
                
                var response = violations.Select(v => 
                {
                    var count = violations.Count(x => x.StudentID == v.StudentID);
                    return MapToResponse(v, count);
                });

                return (true, "All violations retrieved successfully.", 200, response);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving violations: {ex.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetPendingViolationsAsync()
        {
            try
            {
                var violations = await _violationUow.Violations.GetPendingViolations();
                
                var response = violations.Select(v => 
                {
                    var count = violations.Count(x => x.StudentID == v.StudentID);
                    return MapToResponse(v, count);
                });

                return (true, "Pending violations retrieved successfully.", 200, response);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving pending violations: {ex.Message}", 500, null);
            }
        }

        private ViolationResponse MapToResponse(Violation violation, int totalViolations)
        {
            return new ViolationResponse
            {
                ViolationId = violation.ViolationID,
                StudentId = violation.StudentID,
                StudentName = violation.Student?.FullName ?? "Unknown",
                ReportingManagerId = violation.ReportingManagerID,
                ReportingManagerName = violation.ReportingManager?.FullName,
                ViolationAct = violation.ViolationAct,
                ViolationTime = violation.ViolationTime,
                Description = violation.Description,
                Resolution = violation.Resolution,
                TotalViolationsOfStudent = totalViolations
            };
        }
    }
}
