using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkTicketManager.Data;
using WorkTicketManager.DTOs;
using WorkTicketManager.Models;

namespace WorkTicketManager.Controllers
{
    [ApiController]
    [Route("api/app-users")]
    [Authorize]
    public class AppUsersController : ControllerBase
    {
        private readonly WMDbContext _context;

        public AppUsersController(WMDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAppUsers([FromQuery] int? companyId)
        {
            var query = _context.AppUsers
                .Include(u => u.Company)
                .Include(u => u.Role)
                .Include(u => u.Employee)
                .Where(u => u.IsActive)
                .AsQueryable();

            if (companyId.HasValue)
                query = query.Where(u => u.CompanyId == companyId);

            var users = await query
                .OrderBy(u => u.Username)
                .Select(u => new AppUserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    SystemRole = u.SystemRole.ToString(),
                    CompanyId = u.CompanyId,
                    CompanyName = u.Company != null ? u.Company.Name : null,
                    EmployeeId = u.EmployeeId,
                    EmployeeName = u.Employee != null ? u.Employee.FullName : null,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppUser([FromBody] CreateAppUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
                return BadRequest("Username is required");

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Password is required");

            if (await _context.AppUsers.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Username already exists");

            var appUser = new AppUser
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                SystemRole = Enum.Parse<SystemRole>(dto.SystemRole ?? "Default"),
                CompanyId = dto.CompanyId,
                EmployeeId = dto.EmployeeId,
                IsActive = true
            };

            _context.AppUsers.Add(appUser);
            await _context.SaveChangesAsync();

            return Ok(new AppUserDto
            {
                Id = appUser.Id,
                Username = appUser.Username,
                FullName = appUser.FullName,
                SystemRole = appUser.SystemRole.ToString(),
                CompanyId = appUser.CompanyId,
                EmployeeId = appUser.EmployeeId,
                IsActive = appUser.IsActive
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppUser(int id, [FromBody] CreateAppUserDto dto)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

            user.FullName = dto.FullName;
            user.SystemRole = Enum.Parse<SystemRole>(dto.SystemRole ?? "Default");
            user.CompanyId = dto.CompanyId;
            user.EmployeeId = dto.EmployeeId;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _context.SaveChangesAsync();
            return Ok(new AppUserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                SystemRole = user.SystemRole.ToString(),
                CompanyId = user.CompanyId,
                EmployeeId = user.EmployeeId,
                IsActive = user.IsActive
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppUser(int id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}