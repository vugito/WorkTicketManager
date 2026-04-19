using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkTicketManager.Models
{
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        // JSON массив прав: ["tickets.view", "tickets.edit", ...]
        public string Permissions { get; set; } = "[]";

        public int? CompanyId { get; set; }
        public Company? Company { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();
    }
}