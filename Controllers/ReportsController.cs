using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkTicketManager.Data;
using WorkTicketManager.DTOs;

namespace WorkTicketManager.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize] // ← весь контроллер защищён
    public class ReportsController : ControllerBase
    {
        private readonly WMDbContext _context;

        public ReportsController(WMDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<ReportSummaryDto>> GetSummary(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int? companyId)
        {
            if (from > to)
                return BadRequest("'from' must be earlier than 'to'");

            from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            if ((to - from).TotalDays > 365)
                return BadRequest("Maximum range is 1 year");

            var query = _context.Tickets
                .Include(t => t.Status)
                .Include(t => t.Employee)
                    .ThenInclude(e => e.Department)
                .Where(t => t.CreatedAt >= from && t.CreatedAt < to.AddDays(1))
                .AsQueryable();

            if (companyId.HasValue)
                query = query.Where(t => t.CompanyId == companyId);

            var result = new ReportSummaryDto
            {
                From = from,
                To = to,
                TotalTickets = await query.CountAsync(),
                NewTickets = await query.CountAsync(t => t.Status!.Code == "NEW"),
                InProgressTickets = await query.CountAsync(t => t.Status!.Code == "IN_PROGRESS"),
                ClosedTickets = await query.CountAsync(t => t.Status!.Code == "CLOSED"),
                TicketsByDepartment = await query
                    .Where(t => t.Employee != null && t.Employee.Department != null)
                    .GroupBy(t => t.Employee!.Department!.Name)
                    .Select(g => new { g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Key, x => x.Count)
            };

            return Ok(result);
        }

        [HttpGet("chart")]
        public async Task<IActionResult> GetChartData(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] int? companyId)
        {
            if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
                return BadRequest("Invalid date format");

            fromDate = DateTime.SpecifyKind(fromDate.Date, DateTimeKind.Utc);
            toDate = DateTime.SpecifyKind(toDate.Date.AddDays(1), DateTimeKind.Utc);

            var query = _context.Tickets
                .Include(t => t.Status)
                .Where(t => t.CreatedAt >= fromDate && t.CreatedAt < toDate)
                .AsQueryable();

            if (companyId.HasValue)
                query = query.Where(t => t.CompanyId == companyId);

            var tickets = await query.ToListAsync();

            var days = new List<object>();
            var current = fromDate;

            while (current < toDate)
            {
                var next = current.AddDays(1);
                var dayTickets = tickets.Where(t => t.CreatedAt >= current && t.CreatedAt < next).ToList();

                days.Add(new
                {
                    date = current.ToString("dd.MM"),
                    newTickets = dayTickets.Count(t => t.Status?.Code == "NEW"),
                    inProgress = dayTickets.Count(t => t.Status?.Code == "IN_PROGRESS"),
                    closed = dayTickets.Count(t => t.Status?.Code == "CLOSED"),
                    total = dayTickets.Count
                });

                current = next;
            }

            return Ok(days);
        }
    }
}