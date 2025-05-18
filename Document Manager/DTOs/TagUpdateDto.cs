using System.ComponentModel.DataAnnotations;

namespace Document_Manager.DTOs
{
    public class TagUpdateDto
    {
        [StringLength(50)]
        public string? Name { get; set; }
        
        public string? Color { get; set; }
        
        public string? Category { get; set; }
    }
}
