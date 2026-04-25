using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkTicketManager.Data;
using WorkTicketManager.DTOs;
using WorkTicketManager.Services;

namespace WorkTicketManager.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly WMDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(WMDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // POST /api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username and password are required");

            var user = await _context.AppUsers
                .Include(u => u.Company)
                .Include(u => u.Role)
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Username == dto.Username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = await _jwtService.GenerateRefreshToken(user);

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                Username = user.Username,
                FullName = user.FullName ?? user.Username,
                SystemRole = user.SystemRole.ToString(),
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
                EmployeeId = user.EmployeeId,
                IsEmployee = user.EmployeeId != null,
                Permissions = user.Role?.Permissions ?? "[]"
            });
        }

        // POST /api/auth/refresh
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return BadRequest("Refresh token is required");

            var user = await _jwtService.ValidateRefreshToken(dto.RefreshToken);
            if (user == null)
                return Unauthorized("Invalid or expired refresh token");

            var accessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = await _jwtService.GenerateRefreshToken(user);

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                Username = user.Username,
                FullName = user.FullName ?? user.Username,
                SystemRole = user.SystemRole.ToString(),
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
                EmployeeId = user.EmployeeId,
                IsEmployee = user.EmployeeId != null,
                Permissions = user.Role?.Permissions ?? "[]"
            });
        }

        // POST /api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.RefreshToken))
            {
                var token = await _context.RefreshTokens
                    .FirstOrDefaultAsync(r => r.Token == dto.RefreshToken);
                if (token != null)
                {
                    token.IsRevoked = true;
                    await _context.SaveChangesAsync();
                }
            }
            return Ok(new { message = "Logged out successfully" });
        }

        // GET /api/auth/me
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _context.AppUsers
                .Include(u => u.Company)
                .Include(u => u.Role)
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId) && u.IsActive);

            if (user == null) return Unauthorized();

            return Ok(new AuthResponseDto
            {
                AccessToken = "",
                RefreshToken = "",
                Username = user.Username,
                FullName = user.FullName ?? user.Username,
                SystemRole = user.SystemRole.ToString(),
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
                EmployeeId = user.EmployeeId,
                IsEmployee = user.EmployeeId != null,
                Permissions = user.Role?.Permissions ?? "[]"
            });
        }
    }
}