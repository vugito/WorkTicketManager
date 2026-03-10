using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkTicketManager.Data;
using WorkTicketManager.DTOs;
using WorkTicketManager.Models;

namespace WorkTicketManager.Controllers
{
    [ApiController]
    [Route("api/tickets/{ticketId}/comments")]
    public class TicketCommentsController : ControllerBase
    {
        private readonly WMDbContext _context;

        public TicketCommentsController(WMDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketCommentDto>>> GetComments(int ticketId)
        {
            var comments = await _context.TicketComments
                .Where(c => c.TicketId == ticketId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new TicketCommentDto
                {
                    Id = c.Id,
                    TicketId = c.TicketId,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(comments);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int ticketId, [FromBody] CreateTicketCommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Text))
                return BadRequest("Text is required");

            if (!await _context.Tickets.AnyAsync(t => t.Id == ticketId))
                return NotFound("Ticket not found");

            var comment = new TicketComment
            {
                TicketId = ticketId,
                Text = dto.Text.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new TicketCommentDto
            {
                Id = comment.Id,
                TicketId = comment.TicketId,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt
            });
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int ticketId, int commentId)
        {
            var comment = await _context.TicketComments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.TicketId == ticketId);

            if (comment == null) return NotFound();

            _context.TicketComments.Remove(comment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}