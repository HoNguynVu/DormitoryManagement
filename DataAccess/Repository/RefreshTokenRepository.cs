using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(DormitoryDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetRefreshToken(string token)
        {
            return await _dbSet
                .FirstOrDefaultAsync(rt => rt.Token == token && 
                                          rt.RevokedAt == null && 
                                          rt.ExpiresAt > DateTime.UtcNow);
        }

        public void RevokeRefreshToken(string userId)
        {
            var tokens = _dbSet.Where(rt => rt.AccountID == userId && rt.RevokedAt == null);
            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }
        }

        public bool IsValid(string token)
        {
            var refreshToken = _dbSet.FirstOrDefault(rt => rt.Token == token);
            return refreshToken != null && 
                   refreshToken.RevokedAt == null && 
                   refreshToken.ExpiresAt > DateTime.UtcNow;
        }
    }
}
