namespace Document_Manager.Models
{
    public class GroupPermission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Permissions flags
        public bool CanCreate { get; set; }
        public bool CanRead { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public bool IsAdmin { get; set; }
        
        // Navigation properties
        public List<GroupPermissionsUser>? GroupPermissionUsers { get; set; } = new List<GroupPermissionsUser>();
    }
}
