using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.ContractDTOs;
using BusinessObject.DTOs.StudentDTOs;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class StudentService : IStudentService
    {
        private readonly IStudentUow _uow;
        public StudentService(IStudentUow uow)
        {
            _uow = uow;
        }
        public async Task<(bool Success, string Message, int StatusCode, GetStudentDTO? student)> GetStudentByID(string accountId)
        {
            if (string.IsNullOrEmpty(accountId))
                return (false, "Student ID is required.", 400, null);
            var student = await _uow.Students.GetStudentByAccountIdAsync(accountId);
            if (student == null)
            {
                return (false, "Student not found.", 404, null);
            }
            var dto = new GetStudentDTO
            {
                StudentID = student.StudentID,
                FullName = student.FullName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                Address = student.Address,
                SchoolName = student.School.SchoolName,
                CitizenID = student.CitizenID,
                CitizenIDIssuePlace = student.CitizenIDIssuePlace,
                PriorityName = student.Priority.PriorityDescription,
                Gender = student.Gender,
                Relatives = student.Relatives.Select(r => new Relative
                {
                    RelativeID = r.RelativeID,
                    FullName = r.FullName,
                    Relationship = r.Relationship,
                    PhoneNumber = r.PhoneNumber,
                    Occupation = r.Occupation,
                    Address = r.Address
                }).ToList()
            };
            return (true, "Student retrieved successfully.", 200, dto);
        }

        public async Task<(bool Success, string Message, int StatusCode)> UpdateStudent(StudentUpdateInfoDTO infoDTO)
        {
            if (infoDTO == null || string.IsNullOrEmpty(infoDTO.StudentID))
                return (false, "Invalid student data.", 400);
            var student = await _uow.Students.GetByIdAsync(infoDTO.StudentID);
            if (student == null)
                return (false, "Student not found.", 404);
            await _uow.BeginTransactionAsync();
            try
            {
                student.FullName = infoDTO.FullName;
                student.Email = infoDTO.Email;
                student.PhoneNumber = infoDTO.PhoneNumber;
                student.Address = infoDTO.Address;
                student.SchoolID = infoDTO.SchoolID;
                student.PriorityID = infoDTO.PriorityID;
                student.CitizenIDIssuePlace = infoDTO.CitizenIDIssuePlace;
                _uow.Students.Update(student);
                await _uow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Failed to update student: {ex.Message}", 500);
            }
            return (true, "Student updated successfully.", 200);
        }

        public async Task<(bool Success, string Message, int StatusCode)> CreateRelativesForStudent(CreateRelativeDTO relativeDTO)
        {
            if (relativeDTO == null || string.IsNullOrEmpty(relativeDTO.StudentID))
                return (false, "Invalid relative data.", 400);
            var student = await _uow.Students.GetByIdAsync(relativeDTO.StudentID);
            if (student == null)
                return (false, "Student not found.", 404);
            await _uow.BeginTransactionAsync();
            try
            {
                var relative = new Relative
                {
                    RelativeID = "REL-" + IdGenerator.GenerateUniqueSuffix(),
                    FullName = relativeDTO.FullName,
                    Relationship = relativeDTO.Relationship,
                    PhoneNumber = relativeDTO.PhoneNumber,
                    Address = relativeDTO.Address,
                    Occupation = relativeDTO.Occupation,
                    StudentID = relativeDTO.StudentID
                };
                _uow.Relatives.Add(relative);
                await _uow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Failed to create relative: {ex.Message}", 500);
            }
            return (true, "Relative created successfully.", 200);
        }

        public async Task<(bool Success, string Message, int StatusCode)> UpdateRelativesForStudent(UpdateRelativeDTO relativeDTO)
        {
            if (relativeDTO == null || string.IsNullOrEmpty(relativeDTO.RelativeID))
                return (false, "Invalid relative data.", 400);
            var relative = await _uow.Relatives.GetByIdAsync(relativeDTO.RelativeID);
            if (relative == null)
                return (false, "Relative not found.", 404);
            await _uow.BeginTransactionAsync();
            try
            {
                relative.FullName = relativeDTO.FullName;
                relative.Relationship = relativeDTO.Relationship;
                relative.PhoneNumber = relativeDTO.PhoneNumber;
                relative.Address = relativeDTO.Address;
                relative.Occupation = relativeDTO.Occupation;
                _uow.Relatives.Update(relative);
                await _uow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Failed to update relative: {ex.Message}", 500);
            }
            return (true, "Relative updated successfully.", 200);
        }

        public async Task<(bool Success, string Message, int StatusCode)> DeleteRelative(string relativeId)
        {
            if (string.IsNullOrEmpty(relativeId))
                return (false, "Relative ID is required.", 400);
            var relative = await _uow.Relatives.GetByIdAsync(relativeId);
            if (relative == null)
                return (false, "Relative not found.", 404);
            await _uow.BeginTransactionAsync();
            try
            {
                _uow.Relatives.Delete(relative);
                await _uow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return (false, $"Failed to delete relative: {ex.Message}", 500);
            }
            return (true, "Relative deleted successfully.", 200);
        }

        public async Task<(bool Success, string Message, int StatusCode, GetDashboardStudent dto)> GetDashboardByStudentId(string accountId)
        {
            if (string.IsNullOrEmpty(accountId))
                return (false, "Student ID is required.", 400, null);
            var student = await _uow.Students.GetStudentByAccountIdAsync(accountId);
            if (student == null)
            {
                return (false, "Student not found.", 404, null);
            }
            var result = new GetDashboardStudent();
            var contract = await _uow.Contracts.GetLastContractByStudentIdAsync(student.StudentID);
            if (contract == null)
            {
                return (false, "No contract found for the student.", 404, null);
            }
            var contractDetail = new ContractDetailByStudentDto
            {
                ContractID = contract.ContractID,
                Status = contract.ContractStatus,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate ?? DateOnly.MinValue,

                ManagerID = contract.Room?.Building?.ManagerID ?? "N/A",
                ManagerName = contract.Room?.Building?.Manager?.FullName ?? "N/A",
                ManagerEmail = contract.Room?.Building?.Manager?.Email ?? "N/A",
                ManagerPhone = contract.Room?.Building?.Manager?.PhoneNumber ?? "N/A",

                RoomName = contract.Room?.RoomName ?? "N/A",
                BuildingName = contract.Room?.Building?.BuildingName ?? "N/A",
                RoomTypeName = contract.Room?.RoomType?.TypeName ?? "N/A",
                RoomPrice = contract.Room?.RoomType?.Price ?? 0,
            };
            result.CurrentContract = contractDetail;

            result.CountUnpaidBills = await _uow.UtilityBills.CountUnpaidBillByRoomAsync(contract.RoomID);
            result.CountViolations = await _uow.Violations.CountViolationsByStudentId(student.StudentID);
            var insurance = await _uow.HealthInsurances.GetActiveInsuranceByStudentIdAsync(student.StudentID);
            if (insurance != null)
            {
                result.InsuranceStatus = "Active";
                result.InsuranceEndDate = insurance.EndDate;
            }
            else
            {
                result.InsuranceStatus = "Inactive";
                result.InsuranceEndDate = null;
            }
            return (true, "Dashboard data retrieved successfully.", 200, result);
        }
    }
}
