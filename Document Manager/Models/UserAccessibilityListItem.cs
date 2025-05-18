namespace Document_Manager.Models
{
    public class UserAccessibilityListItem
    {
        public Guid Id { get; set; }
        public Guid AccessibilityListId { get; set; }
        public Guid TargetUserId { get; set; }
        public AccessLevel AccessLevel { get; set; }
        
        // Navigation properties
        public UserAccessibilityList? AccessibilityList { get; set; }
        public User? TargetUser { get; set; }
    }
    

}
