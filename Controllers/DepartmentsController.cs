using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkTicketManager.Data;
using WorkTicketManager.DTOs;
using WorkTicketManager.Models;

namespace WorkTicketManager.Controllers
{
    [ApiController]
    [Route("api/departments")]
    [Authorize] // ← весь контроллер защищён
    public class DepartmentsController : ControllerBase
    {
        private readonly WMDbContext _context;

        public DepartmentsController(WMDbContext context) => _context = context;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetDepartments([FromQuery] int? companyId)
        {
            var query = _context.Departments.AsQueryable();

            if (companyId.HasValue)
                query = query.Where(d => d.CompanyId == companyId);

            var departments = await query
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentDto { Id = d.Id, Name = d.Name, CompanyId = d.CompanyId })
                .ToListAsync();

            return Ok(departments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] DepartmentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required");
            if (dto.CompanyId == 0) return BadRequest("CompanyId is required");

            var department = new Department
            {
                Name = dto.Name,
                CompanyId = dto.CompanyId,
                IsActive = true
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            return Ok(new DepartmentDto { Id = department.Id, Name = department.Name, CompanyId = department.CompanyId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] DepartmentDto dto)
        {
            var dep = await _context.Departments.FindAsync(id);
            if (dep == null) return NotFound();

            dep.Name = dto.Name;
            await _context.SaveChangesAsync();
            return Ok(new DepartmentDto { Id = dep.Id, Name = dep.Name, CompanyId = dep.CompanyId });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var dep = await _context.Departments.FindAsync(id);
            if (dep == null) return NotFound();

            var hasActiveEmployees = await _context.Employees
                .AnyAsync(e => e.DepartmentId == id && e.IsActive);

            if (hasActiveEmployees)
                return BadRequest("Нельзя удалить отдел в котором есть активные сотрудники.");

            var inactiveEmployees = await _context.Employees
                .Where(e => e.DepartmentId == id && !e.IsActive)
                .ToListAsync();

            foreach (var emp in inactiveEmployees)
                emp.DepartmentId = null;

            _context.Departments.Remove(dep);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}