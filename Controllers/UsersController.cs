using WorkTicketManager.Data;
using WorkTicketManager.DTOs;
using WorkTicketManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WorkTicketManager.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly WMDbContext _context;

        public UsersController(WMDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Department)
                .Where(u => u.IsActive)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department!.Name
                })
                .ToListAsync();

            return Ok(users);
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName)) return BadRequest("FullName is required");
            if (string.IsNullOrWhiteSpace(dto.Phone)) return BadRequest("Phone is required");

            var user = new User
            {
                FullName = dto.FullName,
                Phone = dto.Phone,
                DepartmentId = dto.DepartmentId,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            dto.Id = user.Id; // возвращаем Id созданного пользователя
            dto.DepartmentName = (await _context.Departments.FindAsync(user.DepartmentId))?.Name ?? "";

            return Ok(dto);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FullName = dto.FullName;
            user.Phone = dto.Phone;
            user.DepartmentId = dto.DepartmentId;

            await _context.SaveChangesAsync();
            return Ok(dto);
        }

        // DELETE: api/users/{id} (soft-delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
