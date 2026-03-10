namespace WorkTicketManager.DTOs
{
    public class TicketCommentDto
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Text { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTicketCommentDto
    {
        public string Text { get; set; } = null!;
    }
}