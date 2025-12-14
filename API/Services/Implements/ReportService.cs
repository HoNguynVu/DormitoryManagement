using API.UnitOfWorks;
using BusinessObject.Entities;
using BusinessObject.DTOs.ReportDTOs;
using API.Services.Interfaces;

namespace API.Services.Implements
{
    public class ReportService : IReportService
    {
        private readonly IContractUow _contractUow;

        public ReportService(IContractUow contractUow)
        {
            _contractUow = contractUow;
        }

        public async Task<IEnumerable<Student>> GetStudentsByPriorityAsync(string? priorityId = null)
        {
            var students = await _contractUow.Students.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(priorityId))
            {
                return students.Where(s => s.PriorityID == priorityId);
            }
            return students.Where(s => !string.IsNullOrWhiteSpace(s.PriorityID));
        }

        public async Task<IEnumerable<ExpiredContractDto>> GetExpiredContractsAsync(DateOnly olderThan)
        {
            var contracts = await _contractUow.Contracts.GetExpiredContractsAsync(olderThan);
            return contracts.Select(c => new ExpiredContractDto
            {
                ContractID = c.ContractID,
                StudentID = c.StudentID,
                StudentName = c.Student?.FullName ?? string.Empty,
                RoomID = c.RoomID,
                EndDate = c.EndDate ?? DateOnly.MinValue,
                ContractStatus = c.ContractStatus
            });
        }
    }
}
