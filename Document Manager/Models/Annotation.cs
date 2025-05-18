namespace Document_Manager.Models
{
    public class Annotation
    {
        public Guid Id { get; set; }
        
        // Document reference
        public Guid DocumentId { get; set; }
        public Document? Document { get; set; }
        
        // Annotation details
        public AnnotationType Type { get; set; }
        public string Content { get; set; } = string.Empty;
        public int? PageNumber { get; set; }
        
        // For highlight or area-specific annotations
        public float? X { get; set; }
        public float? Y { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
        
        // User who made the annotation
        public Guid UserId { get; set; }
        public User? User { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
    
    public enum AnnotationType
    {
        Comment,
        Highlight,
        Drawing,
        TextNote,
        Sticky
    }
}
