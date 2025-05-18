using System.ComponentModel.DataAnnotations;

namespace Document_Manager.DTOs
{
    public class DocumentMoveDto
    {
        [Required]
        public Guid DocumentId { get; set; }
        
        public Guid? TargetFolderId { get; set; }
        
        [Required]
        public bool IsCopy { get; set; } = false;
    }
}
