using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Document_Manager.DTOs
{
    public class DocumentUploadDto
    {
        [Required]
        public IFormFile File { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public List<string>? Tags { get; set; } = new List<string>();
        
        public List<Guid>? TagIds { get; set; } = new List<Guid>();
        
        public Guid? FolderId { get; set; }
    }
}
