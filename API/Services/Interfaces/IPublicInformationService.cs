using BusinessObject.Entities;

namespace API.Services.Interfaces
{
    public interface IPublicInformationService
    {
        Task<(bool Success, string Message, int StatusCode, IEnumerable<School> Schools)> GetSchoolsAsync();
        Task<(bool Success, string Message, int StatusCode, IEnumerable<Priority> Priorities)> GetPrioritiesAsync();
    }
}
