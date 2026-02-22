namespace WorkTicketManager.DTOs
{
    public class LifeTicketDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Department { get; set; } = null!;
        public string User { get; set; } = null!;

        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;

        public string ProblemDescription { get; set; } = null!;

        public string Resolution { get; set; } = null!;

        public DateTime? CompletedAt { get; set; }
    }
}
