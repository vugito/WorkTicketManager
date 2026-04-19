using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkTicketManager.Models
{
    public enum SystemRole
    {
        SuperAdmin,
        Admin,
        Viewer,
        Default
    }

    public class AppUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        public string? FullName { get; set; }

        public SystemRole SystemRole { get; set; } = SystemRole.Default;

        // Привязка к компании (null для SuperAdmin)
        public int? CompanyId { get; set; }
        public Company? Company { get; set; }

        // Кастомная роль (если SystemRole не хватает)
        public int? RoleId { get; set; }
        public Role? Role { get; set; }

        // Привязка к сотруднику (если это работник)
        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}