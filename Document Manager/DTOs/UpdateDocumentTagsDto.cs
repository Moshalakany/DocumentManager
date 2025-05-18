using System.ComponentModel.DataAnnotations;

namespace Document_Manager.DTOs
{
    public class UpdateDocumentTagsDto
    {
        [Required]
        public List<Guid> TagIds { get; set; } = new List<Guid>();
    }
}
