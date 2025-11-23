using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly DormitoryDbContext _context;
        public RefreshTokenRepository(DormitoryDbContext context)
        {
            _context = context;
        }
        public void AddRefreshToken(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
        }
        public async Task<RefreshToken?> GetRefreshToken(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }
        public void RevokeRefreshToken(string userId)
        {
           
            var tokens = _context.RefreshTokens
               .Where(rt => rt.UserId == userId && rt.RevokedAt == null).ToList();
            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }
        }
        public void DeleteRefreshToken(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Remove(refreshToken);
        }
        public void UpdateRefreshToken(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
        }
        public bool IsValid(string token)
        {
            var refreshToken = _context.RefreshTokens
                .FirstOrDefault(rt => rt.Token == token);
            if (refreshToken == null || refreshToken.RevokedAt != null || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }
            return true;
        }
    }
}
