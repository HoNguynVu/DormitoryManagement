using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(DormitoryDbContext context) : base(context)
        {
        }

        public async Task<Student?> GetStudentByEmailAsync(string email)
        {
            return await _dbSet
                .Include(s => s.School)
                .Include(s => s.Priority)
                .Include(s => s.Account)
                .FirstOrDefaultAsync(s => s.Email == email);
        }

        // ✅ Override để thêm eager loading
        public override async Task<Student?> GetByIdAsync(string id)
        {
            return await _dbSet
                .Include(s => s.School)
                .Include(s => s.Priority)
                .Include(s => s.Account)
                .FirstOrDefaultAsync(s => s.StudentID == id);
        }
        public async Task<Student?> GetStudentByAccountIdAsync(string accountId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.AccountID == accountId);
        }
    }
}
