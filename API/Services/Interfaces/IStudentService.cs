using BusinessObject.DTOs.StudentDTOs;
using BusinessObject.Entities;

namespace API.Services.Interfaces
{
    public interface IStudentService
    {
        Task<(bool Success, string Message, int StatusCode, GetStudentDTO? student)> GetStudentByID(string accountId);
        Task<(bool Success, string Message, int StatusCode)> UpdateStudent(StudentUpdateInfoDTO infoDTO);
        Task<(bool Success, string Message, int StatusCode)> CreateRelativesForStudent(CreateRelativeDTO relativeDTO);
        Task<(bool Success, string Message, int StatusCode)> UpdateRelativesForStudent(UpdateRelativeDTO relativeDTO);
        Task<(bool Success, string Message, int StatusCode)> DeleteRelative(string relativeId);
    }
}
