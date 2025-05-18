using System.ComponentModel.DataAnnotations;

namespace Document_Manager.DTOs
{
    public class FolderUpdateDto
    {
        [StringLength(100)]
        public string? Name { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public Guid? ParentFolderId { get; set; }
    }
}
