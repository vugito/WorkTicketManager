using System.ComponentModel.DataAnnotations;

namespace WorkTicketManager.DTOs
{
    public class CreatedTicketDto
    {
        [Required]
        public int UserId { get; set; }

        public int? PriorityId { get; set; }

        [Required]
        [MinLength(5)]
        public string ProblemDescription { get; set; } = null!;

        public DateTime? Deadline { get; set; }
    }
}
