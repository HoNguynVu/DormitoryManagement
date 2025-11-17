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
        public void AddRefreshToken(RefreshToken refreshToken);
        public Task<RefreshToken?> GetRefreshToken(string token);
        public void DeleteRefreshToken(RefreshToken refreshToken);

        public void UpdateRefreshToken(RefreshToken refreshToken);
        public bool IsValid(string token);
    }
}
