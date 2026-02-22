using System.ComponentModel.DataAnnotations;

namespace WorkTicketManager.DTOs
{
    public class DepartmentCreateDto
    {
        [Required]
        public string Name { get; set; } = null!;
    }
}
