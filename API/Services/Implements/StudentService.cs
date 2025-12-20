using API.Services.Interfaces;
using API.UnitOfWorks;
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
        public async Task<(bool Success, string Message, int StatusCode, Student? student)> GetStudentByID(string accountId)
        {
            if (string.IsNullOrEmpty(accountId))
                return (false, "Student ID is required.", 400, null);
            var student = await _uow.Students.GetStudentByAccountIdAsync(accountId);
            if (student == null)
            {
                return (false, "Student not found.", 404, null);
            }
            return (true, "Student retrieved successfully.", 200, student);
        }
    }
}
