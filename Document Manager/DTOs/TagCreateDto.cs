using System.ComponentModel.DataAnnotations;

namespace Document_Manager.DTOs
{
    public class TagCreateDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        public string? Color { get; set; }
        
        public string? Category { get; set; }
    }
}
