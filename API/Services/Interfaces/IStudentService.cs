using BusinessObject.DTOs.StudentDTOs;
using BusinessObject.Entities;

namespace API.Services.Interfaces
{
    public interface IStudentService
    {
        Task<(bool Success, string Message, int StatusCode, GetStudentDTO? student)> GetStudentByID(string accountId);
    }
}
