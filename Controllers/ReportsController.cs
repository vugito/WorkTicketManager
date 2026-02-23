using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkTicketManager.Data;
using WorkTicketManager.DTOs;

namespace WorkTicketManager.Controllers
{
    [ApiController]
    [Route("api/reports")]
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
                    [FromQuery] DateTime to)
        {
            if (from > to)
                return BadRequest("'from' must be earlier than 'to'");

            from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            if ((to - from).TotalDays > 365)
                return BadRequest("Maximum range is 1 year");

            var query = _context.Tickets
                .Include(t => t.Status)
                .Include(t => t.User)
                    .ThenInclude(u => u.Department)
                .Where(t => t.CreatedAt >= from && t.CreatedAt < to.AddDays(1));

            var result = new ReportSummaryDto
            {
                From = from,
                To = to,
                TotalTickets = await query.CountAsync(),
                NewTickets = await query.CountAsync(t => t.Status!.Code == "NEW"),
                InProgressTickets = await query.CountAsync(t => t.Status!.Code == "IN_PROGRESS"),
                ClosedTickets = await query.CountAsync(t => t.Status!.Code == "CLOSED"),
                TicketsByDepartment = await query
                    .Where(t => t.User != null && t.User.Department != null)
                    .GroupBy(t => t.User!.Department!.Name)
                    .Select(g => new { g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Key, x => x.Count)
            };

            return Ok(result);
        }

    }
}
