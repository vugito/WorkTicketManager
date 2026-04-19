using WorkTicketManager.Data;
using WorkTicketManager.DTOs;
using WorkTicketManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace WorkTicketManager.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    [Authorize] // ← весь контроллер защищён
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
                .Include(t => t.Employee)
                    .ThenInclude(e => e.Department)
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
                Department = t.Employee?.Department?.Name ?? "",
                User = t.Employee?.FullName ?? "",
                AnyDesk = t.Employee?.AnyDesk,
                Status = t.Status?.Code ?? "",
                Priority = t.Priority?.Name ?? "",
                ProblemDescription = t.ProblemDescription,
                Resolution = t.Resolution,
                IpAddress = t.IpAddress,
                UserPhone = t.Employee?.Phone
            };
        }

        // =====================
        // GET: Tickets
        // =====================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetTickets([FromQuery] TicketFilterDto filter)
        {
            var query = _context.Tickets
                .Include(t => t.Employee).ThenInclude(e => e.Department)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.StatusCode))
                query = query.Where(t => t.Status.Code == filter.StatusCode);

            if (filter.DepartmentId.HasValue)
                query = query.Where(t => t.Employee.DepartmentId == filter.DepartmentId);

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
        [AllowAnonymous]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("tickets")]
        public async Task<ActionResult<TicketDto>> CreateTicket([FromBody] CreatedTicketDto dto)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == dto.UserId);

            if (employee == null)
                return BadRequest("Invalid employee");

            if (dto.PriorityId.HasValue && !await _context.Priorities.AnyAsync(p => p.Id == dto.PriorityId))
                return BadRequest("Invalid priority");

            var newStatus = await _context.Statuses.SingleOrDefaultAsync(s => s.Code == "NEW");
            if (newStatus == null)
                return StatusCode(500, "Status NEW not found");

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var ticket = new Ticket
            {
                CreatedAt = DateTime.UtcNow,
                EmployeeId = employee.Id,
                CompanyId = employee.Department?.CompanyId ?? 0,
                PriorityId = dto.PriorityId,
                StatusId = newStatus.Id,
                ProblemDescription = dto.ProblemDescription,
                Deadline = dto.Deadline,
                IpAddress = ipAddress
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var createdTicket = await FindTicketAsync(ticket.Id);
            return CreatedAtAction(nameof(GetTicketById), new { id = ticket.Id }, ToDto(createdTicket!));
        }

        // =====================
        // DELETE: Delete Ticket
        // =====================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound("Ticket not found");

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return NoContent();
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

        // =====================
        // PATCH: Update Priority
        // =====================
        [HttpPatch("{id}/priority")]
        public async Task<IActionResult> UpdatePriority(int id, [FromBody] UpdatePriorityDto dto)
        {
            var ticket = await FindTicketAsync(id);
            if (ticket == null)
                return NotFound("Ticket not found");

            if (dto.PriorityId.HasValue && !await _context.Priorities.AnyAsync(p => p.Id == dto.PriorityId))
                return BadRequest("Invalid priority");

            ticket.PriorityId = dto.PriorityId;
            await _context.SaveChangesAsync();
            return Ok(ToDto(ticket));
        }

        // =====================
        // PATCH: Update Deadline
        // =====================
        [HttpPatch("{id}/deadline")]
        public async Task<IActionResult> UpdateDeadline(int id, [FromBody] UpdateDeadlineDto dto)
        {
            var ticket = await FindTicketAsync(id);
            if (ticket == null)
                return NotFound("Ticket not found");

            ticket.Deadline = dto.Deadline.HasValue
                ? DateTime.SpecifyKind(dto.Deadline.Value, DateTimeKind.Utc)
                : null;

            await _context.SaveChangesAsync();
            return Ok(ToDto(ticket));
        }

        // =====================
        // PATCH: Edit Ticket
        // =====================
        [HttpPatch("{id}/edit")]
        public async Task<IActionResult> EditTicket(int id, [FromBody] EditTicketDto dto)
        {
            var ticket = await FindTicketAsync(id);
            if (ticket == null)
                return NotFound("Ticket not found");

            if (dto.ProblemDescription != null)
                ticket.ProblemDescription = dto.ProblemDescription.Trim();

            if (dto.UserId.HasValue)
            {
                var employee = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Id == dto.UserId);
                if (employee == null)
                    return BadRequest("Invalid employee");
                ticket.EmployeeId = employee.Id;
                ticket.CompanyId = employee.Department?.CompanyId ?? ticket.CompanyId;
            }

            if (dto.Resolution != null)
                ticket.Resolution = dto.Resolution.Trim();

            if (dto.StatusCode != null)
            {
                var status = await _context.Statuses.SingleOrDefaultAsync(s => s.Code == dto.StatusCode);
                if (status == null)
                    return BadRequest("Invalid status");
                ticket.StatusId = status.Id;

                if (dto.StatusCode == "IN_PROGRESS" && ticket.StartedAt == null)
                    ticket.StartedAt = DateTime.UtcNow;
                if (dto.StatusCode == "CLOSED" && ticket.CompletedAt == null)
                    ticket.CompletedAt = DateTime.UtcNow;
                if (dto.StatusCode == "NEW")
                {
                    ticket.StartedAt = null;
                    ticket.CompletedAt = null;
                }
            }

            await _context.SaveChangesAsync();
            var updated = await FindTicketAsync(id);
            return Ok(ToDto(updated!));
        }
    }
}