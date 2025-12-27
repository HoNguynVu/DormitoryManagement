using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IViolationRepository : IGenericRepository<Violation>
    {
        Task<IEnumerable<Violation>> GetViolationsByStudentId(string studentId);
        Task<IEnumerable<Violation>> GetPendingViolations();
        Task<IEnumerable<Violation>> GetByManagerId(string managerid);
        Task<int> CountViolationsByStudentId(string studentId);
    }
}