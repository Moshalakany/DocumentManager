namespace Document_Manager.Models
{
    public class GroupPermissionsUser
    {
        public int Id { get; set; }
        public int GroupPermissionId { get; set; }
        public Guid UserId { get; set; }
        //Navigation properties
        public GroupPermission? GroupPermission { get; set; }
        public User? User { get; set; }
    }
}
