using BusinessObject.Entities;

namespace API.Services.Interfaces
{
    public interface IStudentService
    {
        Task<(bool Success, string Message, int StatusCode, Student? student)> GetStudentByID(string accountId);
    }
}
