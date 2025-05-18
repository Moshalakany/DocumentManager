namespace Document_Manager.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string Role { get; set; } = "User"; // Default role is "User"

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        //Navigation properties
        public UserAccessibilityList? AccessibilityList { get; set; }
        public Guid? AccessibilityListId { get; set; }

        public List<GroupPermissionsUser>? GroupPermissionUsers { get; set; } = new List<GroupPermissionsUser>();
        
    }
}
