using BusinessObject.Entities;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Interfaces;

namespace DataAccess.Repository
{
    public class OtpRepository : IOtpRepository
    {
        private readonly DormitoryDbContext _context;
        public OtpRepository(DormitoryDbContext context)
        {
            _context = context;
        }
        public async Task<OtpCode?> GetOtpByEmail(string email)
        {
            return await _context.OtpCodes
                .Include(o => o.User)
                .Where(o => o.User.Email == email && o.IsActive == true)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }
        
        public async Task<OtpCode?> GetActiveOtp(string userId, string purpose)
        {
            return await _context.OtpCodes
                .Where(o => o.UserId == userId && o.Purpose == purpose && o.IsActive == true)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public void AddOtp(OtpCode otpCode)
        {
            _context.OtpCodes.Add(otpCode);
        }
        public void UpdateOtp(OtpCode otpCode)
        {
            _context.OtpCodes.Update(otpCode);
        }
        public void DeleteOtp(OtpCode otpCode)
        {
            _context.OtpCodes.Remove(otpCode);
        }
    }
}
