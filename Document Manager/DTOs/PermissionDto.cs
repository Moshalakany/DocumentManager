using Document_Manager.Models;

namespace Document_Manager.DTOs
{
    public class PermissionDto
    {
        public bool CanView { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDownload { get; set; } = false;
        public bool CanAnnotate { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool CanShare { get; set; } = false;
        
        public AccessLevel GetAccessLevel()
        {
            if (CanView && CanEdit && CanDownload && CanAnnotate && CanDelete && CanShare)
                return AccessLevel.Owner;
            else if (CanView && CanEdit && CanDownload && CanAnnotate)
                return AccessLevel.Edit;
            else if (CanView && CanDownload && CanAnnotate)
                return AccessLevel.Comment;
            else if (CanView && CanDownload)
                return AccessLevel.Download;
            else if (CanView)
                return AccessLevel.View;
            else
                return AccessLevel.View; // Default
        }
        
        public static PermissionDto FromAccessLevel(AccessLevel accessLevel)
        {
            return accessLevel switch
            {
                AccessLevel.View => new PermissionDto { CanView = true },
                AccessLevel.Download => new PermissionDto { CanView = true, CanDownload = true },
                AccessLevel.Comment => new PermissionDto { CanView = true, CanDownload = true, CanAnnotate = true },
                AccessLevel.Edit => new PermissionDto { CanView = true, CanDownload = true, CanAnnotate = true, CanEdit = true },
                AccessLevel.Owner => new PermissionDto { CanView = true, CanDownload = true, CanAnnotate = true, CanEdit = true, CanDelete = true, CanShare = true },
                _ => new PermissionDto { CanView = true }
            };
        }
    }
    
    public class DocumentPermissionDto
    {
        public Guid DocumentId { get; set; }
        public string? DocumentName { get; set; }
        public Guid UserId { get; set; }
        public string? Username { get; set; }
        public PermissionDto Permissions { get; set; } = new PermissionDto();
    }
    
    public class PermissionAssignmentDto
    {
        public Guid DocumentId { get; set; }
        public Guid UserId { get; set; }
        public PermissionDto Permissions { get; set; } = new PermissionDto();
    }
    
    public class GroupPermissionAssignmentDto
    {
        public Guid DocumentId { get; set; }
        public int GroupId { get; set; }
        public PermissionDto Permissions { get; set; } = new PermissionDto();
    }
}
