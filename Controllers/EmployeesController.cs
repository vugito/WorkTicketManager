using WorkTicketManager.Data;
using WorkTicketManager.DTOs;
using WorkTicketManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace WorkTicketManager.Controllers
{
    [ApiController]
    [Route("api/employees")]
    [Authorize] // ← весь контроллер защищён
    public class EmployeesController : ControllerBase
    {
        private readonly WMDbContext _context;

        public EmployeesController(WMDbContext context) => _context = context;

        // GET: api/employees
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetEmployees([FromQuery] int? departmentId, [FromQuery] int? companyId)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Where(e => e.IsActive)
                .AsQueryable();

            if (departmentId.HasValue)
                query = query.Where(e => e.DepartmentId == departmentId);

            if (companyId.HasValue)
                query = query.Where(e => e.Department!.CompanyId == companyId);

            var employees = await query
                .OrderBy(e => e.FullName)
                .Select(e => new UserDto
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Phone = e.Phone,
                    AnyDesk = e.AnyDesk,
                    DepartmentId = e.DepartmentId ?? 0,
                    DepartmentName = e.Department!.Name
                })
                .ToListAsync();

            return Ok(employees);
        }

        // POST: api/employees
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateEmployee([FromBody] UserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName)) return BadRequest("FullName is required");

            var employee = new Employee
            {
                FullName = dto.FullName,
                Phone = dto.Phone,
                DepartmentId = dto.DepartmentId,
                AnyDesk = dto.AnyDesk,
                IsActive = true
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            dto.Id = employee.Id;
            dto.DepartmentName = (await _context.Departments.FindAsync(employee.DepartmentId))?.Name ?? "";

            return Ok(dto);
        }

        // PUT: api/employees/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UserDto dto)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            employee.FullName = dto.FullName;
            employee.Phone = dto.Phone;
            employee.DepartmentId = dto.DepartmentId;
            employee.AnyDesk = dto.AnyDesk;

            await _context.SaveChangesAsync();
            return Ok(dto);
        }

        // DELETE: api/employees/{id} (soft-delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            employee.IsActive = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}