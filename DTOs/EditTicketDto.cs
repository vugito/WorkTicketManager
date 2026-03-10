namespace WorkTicketManager.DTOs
{
    public class EditTicketDto
    {
        public string? ProblemDescription { get; set; }
        public int? UserId { get; set; }
        public string? Resolution { get; set; }
        public string? StatusCode { get; set; }
    }
}