namespace Document_Manager.Models
{
    public class UserAccessibilityListItem
    {
        public Guid Id { get; set; }
        public Guid AccessibilityListId { get; set; }
        public Guid TargetUserId { get; set; }
        public AccessLevel AccessLevel { get; set; }
        
        // Enhanced permissions model
        public bool CanView { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDownload { get; set; } = false;
        public bool CanAnnotate { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool CanShare { get; set; } = false;
        
        // Navigation properties
        public UserAccessibilityList? AccessibilityList { get; set; }
        public User? TargetUser { get; set; }
    }
}
