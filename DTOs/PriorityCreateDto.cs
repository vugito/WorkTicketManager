using System.ComponentModel.DataAnnotations;

namespace WorkTicketManager.DTOs
{
    public class PriorityCreateDto
    {
        [Required]
        public string Name { get; set; } = null!;
    }
}
