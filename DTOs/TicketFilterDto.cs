namespace WorkTicketManager.DTOs
{
    public class TicketFilterDto
    {
        public string? StatusCode { get; set; } // NEW, IN_PROGRESS, CLOSED
        public int? DepartmentId { get; set; }
        public int? PriorityId { get; set; }
        public bool? OnlyOpen { get; set; }
    }
}
