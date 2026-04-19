using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace WorkTicketManager.Models
{
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Department> Departments { get; set; } = new List<Department>();
        public ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();
        public ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}