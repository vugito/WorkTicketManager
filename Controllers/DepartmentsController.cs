using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkTicketManager.Data;
using WorkTicketManager.DTOs;
using WorkTicketManager.Models;

namespace WorkTicketManager.Controllers
{
    [ApiController]
    [Route("api/departments")]
    public class DepartmentsController : ControllerBase
    {
        private readonly WMDbContext _context;

        public DepartmentsController(WMDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Select(d => new DepartmentDto { Id = d.Id, Name = d.Name })
                .ToListAsync();
            return Ok(departments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] DepartmentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required");

            var department = new Department { Name = dto.Name, IsActive = true };
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            return Ok(department);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] DepartmentDto dto)
        {
            var dep = await _context.Departments.FindAsync(id);
            if (dep == null) return NotFound();

            dep.Name = dto.Name;
            await _context.SaveChangesAsync();
            return Ok(dep);
        }

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteDepartment(int id)
        //{
        //    var dep = await _context.Departments.FindAsync(id);
        //    if (dep == null) return NotFound();

        //    _context.Departments.Remove(dep);
        //    await _context.SaveChangesAsync();
        //    return NoContent();
        //}

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var dep = await _context.Departments.FindAsync(id);
            if (dep == null) return NotFound();

            var hasActiveUsers = await _context.Users.AnyAsync(u => u.DepartmentId == id && u.IsActive);
            if (hasActiveUsers)
                return BadRequest("Нельзя удалить отдел в котором есть активные сотрудники.");

            // Деактивированных сотрудников отвязываем от отдела
            var inactiveUsers = await _context.Users
                .Where(u => u.DepartmentId == id && !u.IsActive)
                .ToListAsync();

            foreach (var user in inactiveUsers)
                user.DepartmentId = null;

            _context.Departments.Remove(dep);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
