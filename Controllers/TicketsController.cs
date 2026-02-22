using WorkTicketManager.Data;
using WorkTicketManager.DTOs;
using WorkTicketManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WorkTicketManager.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly WMDbContext _context;

        public TicketsController(WMDbContext context)
        {
            _context = context;
        }

        // =====================
        // Helper
        // =====================
        private async Task<Ticket?> FindTicketAsync(int ticketId)
        {
            return await _context.Tickets
                .Include(t => t.User)
                    .ThenInclude(u => u.Department)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        private TicketDto ToDto(Ticket t)
        {
            return new TicketDto
            {
                Id = t.Id,
                CreatedAt = t.CreatedAt,
                StartedAt = t.StartedAt,
                CompletedAt = t.CompletedAt,
                Deadline = t.Deadline,
                Department = t.User?.Department?.Name ?? "",
                User = t.User?.FullName ?? "",
                Status = t.Status?.Name ?? "",
                Priority = t.Priority?.Name ?? "",
                ProblemDescription = t.ProblemDescription,
                Resolution = t.Resolution
            };
        }

        // =====================
        // GET: Tickets
        // =====================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetTickets([FromQuery] TicketFilterDto filter)
        {
            var query = _context.Tickets
                .Include(t => t.User).ThenInclude(u => u.Department)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.StatusCode))
                query = query.Where(t => t.Status.Code == filter.StatusCode);

            if (filter.DepartmentId.HasValue)
                query = query.Where(t => t.User.DepartmentId == filter.DepartmentId);

            if (filter.PriorityId.HasValue)
                query = query.Where(t => t.PriorityId == filter.PriorityId);

            if (filter.OnlyOpen == true)
                query = query.Where(t => t.Status.Code != "CLOSED");

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tickets.Select(ToDto));
        }

        // =====================
        // GET: Ticket by Id
        // =====================
        [HttpGet("{id}")]
        public async Task<ActionResult<TicketDto>> GetTicketById(int id)
        {
            var ticket = await FindTicketAsync(id);
            if (ticket == null)
                return NotFound("Ticket not found");

            return Ok(ToDto(ticket));
        }

        // =====================
        // POST: Create Ticket
        // =====================
        [HttpPost]
        public async Task<ActionResult<TicketDto>> CreateTicket([FromBody] CreatedTicketDto dto)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId))
                return BadRequest("Invalid user");

            if (!await _context.Priorities.AnyAsync(p => p.Id == dto.PriorityId))
                return BadRequest("Invalid priority");

            var newStatus = await _context.Statuses.SingleOrDefaultAsync(s => s.Code == "NEW");
            if (newStatus == null)
                return StatusCode(500, "Status NEW not found");

            var ticket = new Ticket
            {
                CreatedAt = DateTime.UtcNow,
                UserId = dto.UserId,
                PriorityId = dto.PriorityId,
                StatusId = newStatus.Id,
                ProblemDescription = dto.ProblemDescription,
                Deadline = dto.Deadline
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // ВАЖНО: перечитать тикет с навигациями
            var createdTicket = await FindTicketAsync(ticket.Id);

            return CreatedAtAction(
                nameof(GetTicketById),
                new { id = ticket.Id },
                ToDto(createdTicket!)
            );
        }

        // =====================
        // POST: Start Ticket
        // =====================
        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartTicket(int id)
        {
            var ticket = await FindTicketAsync(id);
            if (ticket == null)
                return NotFound("Ticket not found");

            if (ticket.Status?.Code != "NEW")
                return BadRequest("Only NEW tickets can be started");

            var inProgressStatus = await _context.Statuses.SingleAsync(s => s.Code == "IN_PROGRESS");

            ticket.StatusId = inProgressStatus.Id;
            ticket.StartedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ToDto(ticket));
        }

        // =====================
        // POST: Close Ticket
        // =====================
        [HttpPost("{id}/close")]
        public async Task<IActionResult> CloseTicket(int id, [FromBody] CloseTicketDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Resolution))
                return BadRequest("Resolution is required");

            var ticket = await FindTicketAsync(id);
            if (ticket == null)
                return NotFound("Ticket not found");

            if (ticket.Status?.Code != "IN_PROGRESS")
                return BadRequest("Only IN_PROGRESS tickets can be closed");

            var closedStatus = await _context.Statuses.SingleAsync(s => s.Code == "CLOSED");

            ticket.StatusId = closedStatus.Id;
            ticket.CompletedAt = DateTime.UtcNow;
            ticket.Resolution = dto.Resolution;

            await _context.SaveChangesAsync();
            return Ok(ToDto(ticket));
        }
    }
}
