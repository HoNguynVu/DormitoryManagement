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
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {
        public AccountRepository(DormitoryDbContext context) : base(context)
        {
        }
        public async Task<Account?> GetAccountByStudentId(string studentId)
        {
            return await _context.Students
                .Where(s => s.StudentID == studentId)
                .Select(s => s.Account)
                .FirstOrDefaultAsync();
        }
        public async Task<Account?> GetAccountByUsername(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(a => a.Username == username);
        }
        public async Task<Account?> GetAccountByOtp(string otp)
        {
            return await _context.OtpCodes
                .Where(o => o.Code == otp)
                .Select(o => o.Account)
                .FirstOrDefaultAsync();
        }
    }
}
