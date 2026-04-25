namespace WorkTicketManager.DTOs
{
    public class AppUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string? FullName { get; set; }
        public string SystemRole { get; set; } = null!;
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateAppUserDto
    {
        public string Username { get; set; } = null!;
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? SystemRole { get; set; }
        public int? CompanyId { get; set; }
        public int? EmployeeId { get; set; }
    }
}