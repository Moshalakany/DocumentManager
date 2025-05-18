namespace Document_Manager.Models
{
    public class AccessibilityListItem
    {
        public Guid Id { get; set; }


        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        //Navigation properties
        public DocumentAccessibilityList? AccessibilityList { get; set; }
    }

    public class DocumentAccessibilityListItem
    {
        public Guid Id { get; set; }
        public Guid AccessibilityListId { get; set; }
        public Guid UserId { get; set; }
        public AccessLevel AccessLevel { get; set; }
        
        // Enhanced permissions model
        public bool CanView { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDownload { get; set; } = false;
        public bool CanAnnotate { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool CanShare { get; set; } = false;
        
        // Navigation properties
        public DocumentAccessibilityList? AccessibilityList { get; set; }
        public User? User { get; set; }
    }

    // Add this if it doesn't exist in another file
    public enum AccessLevel
    {
        View,       // Can only view
        Download,   // Can view and download
        Comment,    // Can view, download, and comment
        Edit,       // Can view, download, comment, and edit
        Owner       // Full access, including sharing and deleting
    }
}
