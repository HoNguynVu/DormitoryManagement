using BusinessObject.Entities;
using BusinessObject.DTOs.ReportDTOs;

namespace API.Services.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<Student>> GetStudentsByPriorityAsync(string? priorityId = null);
        Task<IEnumerable<ExpiredContractDto>> GetExpiredContractsAsync(DateOnly olderThan);
    }
}
