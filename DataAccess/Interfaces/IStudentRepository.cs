using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
        Task<Student?> GetStudentByEmailAsync(string email);
        Task<Student?> GetStudentByAccountIdAsync(string accountId);
        Task<IEnumerable<Student>> GetStudentsWithPriorityAsync(string? priorityId);
        Task<int> CountStudentByManagerIdAsync(string managerId);
    }
}
