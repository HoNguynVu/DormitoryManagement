using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IRefreshTokenRepository
    {
        void AddRefreshToken(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshToken(string token);
        void RevokeRefreshToken(string userId);
        void DeleteRefreshToken(RefreshToken refreshToken);

        void UpdateRefreshToken(RefreshToken refreshToken);
        bool IsValid(string token);
    }
}
