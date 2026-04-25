using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkTicketManager.Data;
using WorkTicketManager.DTOs;
using WorkTicketManager.Models;

namespace WorkTicketManager.Controllers
{
    [ApiController]
    [Route("api/companies")]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly WMDbContext _context;

        public CompaniesController(WMDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _context.Companies
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new CompanyDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return Ok(companies);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required");

            var company = new Company
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = true
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return Ok(new CompanyDto { Id = company.Id, Name = company.Name, Description = company.Description, IsActive = company.IsActive });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] CompanyDto dto)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            company.Name = dto.Name;
            company.Description = dto.Description;
            await _context.SaveChangesAsync();

            return Ok(new CompanyDto { Id = company.Id, Name = company.Name, Description = company.Description, IsActive = company.IsActive });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            company.IsActive = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}