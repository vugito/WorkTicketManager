using System.ComponentModel.DataAnnotations;

namespace WorkTicketManager.DTOs
{
    public class CloseTicketDto
    {
        [Required]
        public string Resolution { get; set; } = null!;
        
    }
}
