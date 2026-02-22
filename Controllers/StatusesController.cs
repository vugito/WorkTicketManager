using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkTicketManager.Data;
using WorkTicketManager.DTOs;

namespace WorkTicketManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusesController : ControllerBase
    {
        private readonly WMDbContext _context;
        public StatusesController(WMDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetStatuses()
        {
            var statuses = await _context.Statuses
                .Select(s => new StatusDto { Id = s.Id, Name = s.Name })
                .ToListAsync();
            return Ok(statuses);
        }
    }
}
