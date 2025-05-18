namespace Document_Manager.Models
{
    public class UserAccessibilityList
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        
        // Navigation property for associated user
        public User? User { get; set; }
        
        // Navigation properties
        public List<UserAccessibilityListItem>? AccessibilityListItems { get; set; } = new List<UserAccessibilityListItem>();
    }
}
