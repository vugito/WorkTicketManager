using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkTicketManager.Data;
using WorkTicketManager.DTOs;
using WorkTicketManager.Models;

namespace WorkTicketManager.Controllers
{
    [ApiController]
    [Route("api/priorities")]
    public class PrioritiesController : ControllerBase
    {
        private readonly WMDbContext _context;

        public PrioritiesController(WMDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PriorityDto>>> GetPriorities()
        {
            return Ok(await _context.Priorities
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new PriorityDto
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreatePriority([FromBody] PriorityCreateDto dto)
        {
            if (await _context.Priorities.AnyAsync(p => p.Name == dto.Name))
                return BadRequest("Priority already exists");

            var priority = new Priority
            {
                Name = dto.Name,
                IsActive = true
            };

            _context.Priorities.Add(priority);
            await _context.SaveChangesAsync();

            return Ok(new PriorityDto
            {
                Id = priority.Id,
                Name = priority.Name
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePriority(int id, [FromBody] PriorityCreateDto dto)
        {
            var priority = await _context.Priorities.FindAsync(id);
            if (priority == null || !priority.IsActive)
                return NotFound();

            if (await _context.Priorities.AnyAsync(p => p.Name == dto.Name && p.Id != id))
                return BadRequest("Priority already exists");

            priority.Name = dto.Name;
            await _context.SaveChangesAsync();

            return Ok(new PriorityDto
            {
                Id = priority.Id,
                Name = priority.Name
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePriority(int id)
        {
            var priority = await _context.Priorities.FindAsync(id);
            if (priority == null || !priority.IsActive)
                return NotFound();

            priority.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}
