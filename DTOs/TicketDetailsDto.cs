namespace WorkTicketManager.DTOs
{
    public class TicketDetailsDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserFullName { get; set; } = null!;
        public string DepartmentName { get; set; } = null!;

        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;

        public string ProblemDescription { get; set; } = null!;
        public DateTime? Deadline { get; set; }

        public DateTime? CompletedAt { get; set; }
        public string? Resolution { get; set; }
    }
}
