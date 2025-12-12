using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class OtpRepository : GenericRepository<OtpCode>, IOtpRepository
    {
        public OtpRepository(DormitoryDbContext context) : base(context)
        {
        }

        public async Task<OtpCode?> GetOtpByEmail(string email)
        {
            return await _dbSet
                .Where(o => o.Account.Email == email)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<OtpCode?> GetActiveOtp(string userId, string purpose)
        {
            return await _dbSet
                .Where(o => o.AccountID == userId && 
                           o.Purpose == purpose && 
                           o.IsActive)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
