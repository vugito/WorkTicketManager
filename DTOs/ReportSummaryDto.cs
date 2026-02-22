namespace WorkTicketManager.DTOs
{
    public class ReportSummaryDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int TotalTickets { get; set; }
        public int NewTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ClosedTickets { get; set; }

        public Dictionary<string, int> TicketsByDepartment { get; set; } = new();
    }
}
