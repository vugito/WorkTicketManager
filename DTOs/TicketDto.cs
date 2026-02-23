namespace WorkTicketManager.DTOs
{
    public class TicketDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }       // новое поле
        public DateTime? CompletedAt { get; set; }
        public DateTime? Deadline { get; set; }

        public string Department { get; set; } = null!;
        public string User { get; set; } = null!;

        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;

        public string? ProblemDescription { get; set; } // удобно фронтенду
        public string? Resolution { get; set; }         // для CLOSED тикетов
        public string? AnyDesk { get; set; }
    }
}
