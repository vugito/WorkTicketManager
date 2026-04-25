namespace WorkTicketManager.DTOs
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string SystemRole { get; set; } = null!;
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int? EmployeeId { get; set; }
        public bool IsEmployee { get; set; }
        public string Permissions { get; set; } = "[]";
    }
}