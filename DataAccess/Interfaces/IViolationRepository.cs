using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IViolationRepository
    {
        Task<Violation?> GetViolationById(string violationId);
        Task<IEnumerable<Violation>> GetViolationsByStudentId(string studentId);
        Task<IEnumerable<Violation>> GetAllViolations();
        Task<IEnumerable<Violation>> GetPendingViolations();
        Task<int> CountViolationsByStudentId(string studentId);
        void AddViolation(Violation violation);
        void UpdateViolation(Violation violation);
        void DeleteViolation(Violation violation);
    }
}