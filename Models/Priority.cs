using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkTicketManager.Models
{
    public class Priority
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
