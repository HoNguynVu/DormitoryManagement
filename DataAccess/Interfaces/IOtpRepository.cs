using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IOtpRepository
    {
        Task<OtpCode?> GetOtpByEmail(string email);
        Task<OtpCode?> GetActiveOtp(string userId, string purpose);
        void AddOtp(OtpCode otpCode);
        void UpdateOtp(OtpCode otpCode);
        void DeleteOtp(OtpCode otpCode);
    }
}
