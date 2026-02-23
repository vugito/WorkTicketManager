using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkTicketManager.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public int? DepartmentId { get; set; }

        public Department? Department { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    }
}
