using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Document_Manager.Models
{
    public class Document 
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        
        // Added for file management and validation
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
        public bool IsValidated { get; set; } = false;
        
        // Added for folder management
        public Guid? FolderId { get; set; }
        public Folder? Folder { get; set; }
        
        // Added for versioning
        public int Version { get; set; } = 1;
        public bool IsCurrentVersion { get; set; } = true;
        public Guid? OriginalDocumentId { get; set; }
        public Document? OriginalDocument { get; set; }
        public List<Document>? Versions { get; set; } = new List<Document>();
        
        // Added for annotations
        public List<Annotation>? Annotations { get; set; } = new List<Annotation>();
        
        // Added for in-document search
        public bool HasOcr { get; set; } = false;
        public string? OcrText { get; set; }
        
        //Navigation properties
        public DocumentAccessibilityList? AccessibilityList { get; set; }
        public Guid? AccessibilityListId { get; set; }
        public List<Tag>? Tags { get; set; } = new List<Tag>();

        // Add user reference
        public User? CreatedBy { get; set; }
        public Guid? CreatedById { get; set; }
    }
}
