using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkTicketManager.Data;
using WorkTicketManager.Models;

namespace WorkTicketManager.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        private readonly WMDbContext _context;

        public JwtService(IConfiguration config, WMDbContext context)
        {
            _config = config;
            _context = context;
        }

        public string GenerateAccessToken(AppUser user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("systemRole", user.SystemRole.ToString()),
                new Claim("companyId", user.CompanyId?.ToString() ?? ""),
                new Claim("employeeId", user.EmployeeId?.ToString() ?? ""),
                new Claim("fullName", user.FullName ?? ""),
            };

            var expires = DateTime.UtcNow.AddMinutes(
                double.Parse(_config["Jwt:ExpiresInMinutes"] ?? "60"));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<RefreshToken> GenerateRefreshToken(AppUser user)
        {
            // Отзываем старые refresh токены
            var oldTokens = await _context.RefreshTokens
                .Where(r => r.AppUserId == user.Id && !r.IsRevoked)
                .ToListAsync();

            foreach (var old in oldTokens)
                old.IsRevoked = true;

            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                AppUserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<AppUser?> ValidateRefreshToken(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(r => r.AppUser)
                    .ThenInclude(u => u.Company)
                .Include(r => r.AppUser)
                    .ThenInclude(u => u.Role)
                .Include(r => r.AppUser)
                    .ThenInclude(u => u.Employee)
                .FirstOrDefaultAsync(r =>
                    r.Token == token &&
                    !r.IsRevoked &&
                    r.ExpiresAt > DateTime.UtcNow);

            return refreshToken?.AppUser;
        }
    }
}