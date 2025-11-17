using API.Services.Helpers;
using BusinessObject.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;

namespace API.Services.Implements
{
    public partial class AuthService
    {
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private RefreshToken CreateRefreshToken(string accountId)
        {
            return new RefreshToken
            {
                TokenId = "RT-" + IdGenerator.GenerateUniqueSuffix(),
                Token = GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                UserId = accountId
            };
        }


        private string GenerateJwtToken(Account user)
        {
            var claims = new List<Claim>
            {
                 new Claim("userid", user.UserId),
                new Claim("username", user.Username),
                new Claim("role", user.Role) // Vai trò người dùng
            }; // Tạo danh sách claims
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
              _configuration.GetValue<string>("AppSettings:Token")!
          ));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);// Tạo chữ ký
            var tokenDescriptor = new JwtSecurityToken(
                 issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
                 audience: _configuration.GetValue<string>("AppSettings:Audience"),
                 claims: claims, // Thông tin người dùng
                 expires: DateTime.UtcNow.AddHours(1), // Thời gian hết hạn
                 signingCredentials: creds // Chữ ký
             ); // Tạo token
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor); // Trả về token dưới dạng chuỗi
        }
    }
}
