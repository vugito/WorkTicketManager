using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkTicketManager.Models
{
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        // Для быстрой фильтрации по компании
        public int CompanyId { get; set; }
        public Company? Company { get; set; }

        [Required]
        public string ProblemDescription { get; set; } = null!;

        public int StatusId { get; set; }
        public Status? Status { get; set; }

        public int? PriorityId { get; set; }
        public Priority? Priority { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string? Resolution { get; set; }
        public string? IpAddress { get; set; }

        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    }
}