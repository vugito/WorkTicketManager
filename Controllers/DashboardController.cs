using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkTicketManager.Data;
using WorkTicketManager.DTOs;

namespace WorkTicketManager.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize] // ← весь контроллер защищён
    public class DashboardController : ControllerBase
    {
        private readonly WMDbContext _context;

        public DashboardController(WMDbContext context)
        {
            _context = context;
        }

        [HttpGet("life-monitor")]
        public async Task<ActionResult<LifeMonitorDto>> GetLifeMonitor([FromQuery] int? companyId)
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = todayStart.AddDays(-6);
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var statusIds = await _context.Statuses
                .Where(s => s.Code == "NEW" || s.Code == "IN_PROGRESS" || s.Code == "CLOSED")
                .ToDictionaryAsync(s => s.Code, s => s.Id);

            var query = _context.Tickets.AsQueryable();
            if (companyId.HasValue)
                query = query.Where(t => t.CompanyId == companyId);

            var dto = new LifeMonitorDto
            {
                NewTickets = await query.CountAsync(t => t.StatusId == statusIds["NEW"]),
                InProgressTickets = await query.CountAsync(t => t.StatusId == statusIds["IN_PROGRESS"]),
                ClosedToday = await query.CountAsync(t =>
                    t.StatusId == statusIds["CLOSED"] &&
                    t.CompletedAt >= todayStart),
                TotalToday = await query.CountAsync(t => t.CreatedAt >= todayStart),
                TotalThisWeek = await query.CountAsync(t => t.CreatedAt >= weekStart),
                TotalThisMonth = await query.CountAsync(t => t.CreatedAt >= monthStart),
                LastUpdated = now
            };

            return Ok(dto);
        }

        [HttpGet("life-tickets")]
        public async Task<ActionResult<IEnumerable<LifeTicketDto>>> GetLifeTickets(
            [FromQuery] string? status = null,
            [FromQuery] string? period = null,
            [FromQuery] int? companyId = null)
        {
            var query = _context.Tickets
                .Include(t => t.Employee).ThenInclude(e => e.Department)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .AsQueryable();

            if (companyId.HasValue)
                query = query.Where(t => t.CompanyId == companyId);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status!.Code == status);

            if (!string.IsNullOrWhiteSpace(period))
            {
                var now = DateTime.UtcNow;
                var todayStart = now.Date;

                query = period.Trim().ToLowerInvariant() switch
                {
                    "today" => query.Where(t =>
                        t.CreatedAt >= todayStart &&
                        t.CreatedAt < todayStart.AddDays(1)),
                    "week" => query.Where(t => t.CreatedAt >= todayStart.AddDays(-7)),
                    _ => query
                };
            }

            var result = await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new LifeTicketDto
                {
                    Id = t.Id,
                    CreatedAt = t.CreatedAt,
                    Department = t.Employee!.Department!.Name,
                    User = t.Employee.FullName,
                    Status = t.Status!.Code,
                    Priority = t.Priority!.Name,
                    ProblemDescription = t.ProblemDescription,
                    Resolution = t.Resolution,
                    StartedAt = t.StartedAt,
                    Deadline = t.Deadline,
                    AnyDesk = t.Employee!.AnyDesk,
                    UserPhone = t.Employee!.Phone,
                    IpAddress = t.IpAddress,
                    CompletedAt = t.CompletedAt
                })
                .ToListAsync();

            return Ok(result);
        }
    }
}