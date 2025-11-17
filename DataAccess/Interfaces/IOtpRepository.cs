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
        Task<OtpCode?> GetOtpById(int otpId);
        Task<OtpCode?> GetOtpByCode(string code);
        void AddOtp(OtpCode otpCode);
        void UpdateOtp(OtpCode otpCode);
        void DeleteOtp(OtpCode otpCode);
    }
}
