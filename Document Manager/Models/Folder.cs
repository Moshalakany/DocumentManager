namespace Document_Manager.Models
{
    public class Folder
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Hierarchy support
        public Guid? ParentFolderId { get; set; }
        public Folder? ParentFolder { get; set; }
        public List<Folder>? SubFolders { get; set; } = new List<Folder>();
        
        // Documents in this folder
        public List<Document>? Documents { get; set; } = new List<Document>();
        
        // Ownership and timestamps
        public Guid? OwnerId { get; set; }
        public User? Owner { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Access control
        public DocumentAccessibilityList? AccessibilityList { get; set; }
        public Guid? AccessibilityListId { get; set; }
    }
}
