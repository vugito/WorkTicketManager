namespace WorkTicketManager.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }
}
