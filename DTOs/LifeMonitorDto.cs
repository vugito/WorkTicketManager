namespace WorkTicketManager.DTOs
{
    public class LifeMonitorDto
    {
        public int NewTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ClosedToday { get; set; }

        public int TotalToday { get; set; }
        public int TotalThisWeek { get; set; }
        public int TotalThisMonth { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
