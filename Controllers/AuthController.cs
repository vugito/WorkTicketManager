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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username and password are required");

            var user = await _context.AppUsers
                .Include(u => u.Company)
                .Include(u => u.Role)
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Username == dto.Username && u.IsActive);

            if (user == null)
                return Unauthorized("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
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

        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _context.AppUsers
                .Include(u => u.Company)
                .Include(u => u.Role)
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null) return Unauthorized();

            return Ok(new AuthResponseDto
            {
                Token = "",
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