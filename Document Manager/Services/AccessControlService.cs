using Document_Manager.Data;
using Document_Manager.DTOs;
using Document_Manager.Models;
using Document_Manager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Document_Manager.Services
{
    public class AccessControlService : IAccessControlService
    {
        private readonly AppDbContextSQL _context;

        public AccessControlService(AppDbContextSQL context)
        {
            _context = context;
        }

        // Document permissions
        public async Task<bool> AssignDocumentPermissionToUserAsync(Guid documentId, Guid userId, PermissionDto permissions)
        {
            var document = await _context.Documents
                .Include(d => d.AccessibilityList)
                .ThenInclude(a => a.AccessibilityListItems)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                return false;

            // Create accessibility list if it doesn't exist
            if (document.AccessibilityList == null)
            {
                document.AccessibilityList = new DocumentAccessibilityList
                {
                    Id = Guid.NewGuid(),
                    DocumentId = documentId,
                    AccessibilityListItems = new List<DocumentAccessibilityListItem>()
                };
            }

            // Check if permission already exists for user
            var existingPermission = document.AccessibilityList.AccessibilityListItems?
                .FirstOrDefault(a => a.UserId == userId);

            if (existingPermission != null)
            {
                // Update existing permission
                existingPermission.CanView = permissions.CanView;
                existingPermission.CanEdit = permissions.CanEdit;
                existingPermission.CanDownload = permissions.CanDownload;
                existingPermission.CanAnnotate = permissions.CanAnnotate;
                existingPermission.CanDelete = permissions.CanDelete;
                existingPermission.CanShare = permissions.CanShare;
                existingPermission.AccessLevel = permissions.GetAccessLevel();
            }
            else
            {
                // Add new permission
                var accessListItem = new DocumentAccessibilityListItem
                {
                    Id = Guid.NewGuid(),
                    AccessibilityListId = document.AccessibilityList.Id,
                    UserId = userId,
                    CanView = permissions.CanView,
                    CanEdit = permissions.CanEdit,
                    CanDownload = permissions.CanDownload,
                    CanAnnotate = permissions.CanAnnotate,
                    CanDelete = permissions.CanDelete,
                    CanShare = permissions.CanShare,
                    AccessLevel = permissions.GetAccessLevel()
                };

                document.AccessibilityList.AccessibilityListItems?.Add(accessListItem);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignDocumentPermissionToGroupAsync(Guid documentId, int groupId, PermissionDto permissions)
        {
            var document = await _context.Documents
                .Include(d => d.AccessibilityList)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                return false;

            var group = await _context.GroupPermissions
                .Include(g => g.GroupPermissionUsers)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return false;

            // For each user in the group, assign the permissions
            foreach (var userGroup in group.GroupPermissionUsers)
            {
                await AssignDocumentPermissionToUserAsync(documentId, userGroup.UserId, permissions);
            }

            return true;
        }

        public async Task<bool> RemoveDocumentPermissionForUserAsync(Guid documentId, Guid userId)
        {
            var document = await _context.Documents
                .Include(d => d.AccessibilityList)
                .ThenInclude(a => a.AccessibilityListItems)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document?.AccessibilityList?.AccessibilityListItems == null)
                return false;

            var permission = document.AccessibilityList.AccessibilityListItems
                .FirstOrDefault(a => a.UserId == userId);

            if (permission == null)
                return false;

            document.AccessibilityList.AccessibilityListItems.Remove(permission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveDocumentPermissionForGroupAsync(Guid documentId, int groupId)
        {
            var group = await _context.GroupPermissions
                .Include(g => g.GroupPermissionUsers)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group?.GroupPermissionUsers == null)
                return false;

            // For each user in the group, remove the permissions
            foreach (var userGroup in group.GroupPermissionUsers)
            {
                await RemoveDocumentPermissionForUserAsync(documentId, userGroup.UserId);
            }

            return true;
        }

        public async Task<List<DocumentPermissionDto>> GetDocumentPermissionsAsync(Guid documentId)
        {
            var document = await _context.Documents
                .Include(d => d.AccessibilityList)
                .ThenInclude(a => a.AccessibilityListItems)
                .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document?.AccessibilityList?.AccessibilityListItems == null)
                return new List<DocumentPermissionDto>();

            return document.AccessibilityList.AccessibilityListItems
                .Select(a => new DocumentPermissionDto
                {
                    DocumentId = documentId,
                    DocumentName = document.Name,
                    UserId = a.UserId,
                    Username = a.User?.Username,
                    Permissions = new PermissionDto
                    {
                        CanView = a.CanView,
                        CanEdit = a.CanEdit,
                        CanDownload = a.CanDownload,
                        CanAnnotate = a.CanAnnotate,
                        CanDelete = a.CanDelete,
                        CanShare = a.CanShare
                    }
                }).ToList();
        }

        public async Task<List<DocumentPermissionDto>> GetUserAccessibleDocumentsAsync(Guid userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return new List<DocumentPermissionDto>();

            // Get all documents where:
            // 1. User is the owner
            // 2. User has explicit permissions
            var documents = await _context.Documents
                .Include(d => d.AccessibilityList)
                .ThenInclude(a => a.AccessibilityListItems)
                .Where(d => d.CreatedById == userId ||
                            d.AccessibilityList.AccessibilityListItems
                                .Any(a => a.UserId == userId && a.CanView))
                .ToListAsync();

            var result = new List<DocumentPermissionDto>();

            foreach (var document in documents)
            {
                bool isOwner = document.CreatedById == userId;
                var permission = document.AccessibilityList?.AccessibilityListItems?
                    .FirstOrDefault(a => a.UserId == userId);

                result.Add(new DocumentPermissionDto
                {
                    DocumentId = document.Id,
                    DocumentName = document.Name,
                    UserId = userId,
                    Username = user.Username,
                    Permissions = isOwner
                        ? new PermissionDto { CanView = true, CanEdit = true, CanDownload = true, CanAnnotate = true, CanDelete = true, CanShare = true }
                        : permission != null
                            ? new PermissionDto
                            {
                                CanView = permission.CanView,
                                CanEdit = permission.CanEdit,
                                CanDownload = permission.CanDownload,
                                CanAnnotate = permission.CanAnnotate,
                                CanDelete = permission.CanDelete,
                                CanShare = permission.CanShare
                            }
                            : new PermissionDto { CanView = true } // Default view permission
                });
            }

            return result;
        }

        // Authorization check methods
        public async Task<bool> CanUserViewDocumentAsync(Guid documentId, Guid userId)
        {
            // Admin can view all documents
            if (await IsUserAdminAsync(userId))
                return true;

            // Document owner can view document
            if (await IsDocumentOwnerAsync(documentId, userId))
                return true;

            // Check explicit permissions
            var permissions = await GetUserDocumentPermissionsAsync(documentId, userId);
            return permissions.CanView;
        }

        public async Task<bool> CanUserEditDocumentAsync(Guid documentId, Guid userId)
        {
            // Admin can edit all documents
            if (await IsUserAdminAsync(userId))
                return true;

            // Document owner can edit document
            if (await IsDocumentOwnerAsync(documentId, userId))
                return true;

            // Check explicit permissions
            var permissions = await GetUserDocumentPermissionsAsync(documentId, userId);
            return permissions.CanEdit;
        }

        public async Task<bool> CanUserDownloadDocumentAsync(Guid documentId, Guid userId)
        {
            // Admin can download all documents
            if (await IsUserAdminAsync(userId))
                return true;

            // Document owner can download document
            if (await IsDocumentOwnerAsync(documentId, userId))
                return true;

            // Check explicit permissions
            var permissions = await GetUserDocumentPermissionsAsync(documentId, userId);
            return permissions.CanDownload;
        }

        public async Task<bool> CanUserDeleteDocumentAsync(Guid documentId, Guid userId)
        {
            // Admin can delete all documents
            if (await IsUserAdminAsync(userId))
                return true;

            // Document owner can delete document
            if (await IsDocumentOwnerAsync(documentId, userId))
                return true;

            // Check explicit permissions
            var permissions = await GetUserDocumentPermissionsAsync(documentId, userId);
            return permissions.CanDelete;
        }

        public async Task<bool> CanUserShareDocumentAsync(Guid documentId, Guid userId)
        {
            // Admin can share all documents
            if (await IsUserAdminAsync(userId))
                return true;

            // Document owner can share document
            if (await IsDocumentOwnerAsync(documentId, userId))
                return true;

            // Check explicit permissions
            var permissions = await GetUserDocumentPermissionsAsync(documentId, userId);
            return permissions.CanShare;
        }

        public async Task<bool> CanUserAnnotateDocumentAsync(Guid documentId, Guid userId)
        {
            // Admin can annotate all documents
            if (await IsUserAdminAsync(userId))
                return true;

            // Document owner can annotate document
            if (await IsDocumentOwnerAsync(documentId, userId))
                return true;

            // Check explicit permissions
            var permissions = await GetUserDocumentPermissionsAsync(documentId, userId);
            return permissions.CanAnnotate;
        }

        // Helper methods
        public async Task<PermissionDto> GetUserDocumentPermissionsAsync(Guid documentId, Guid userId)
        {
            // If user is admin, they have all permissions
            if (await IsUserAdminAsync(userId))
            {
                return new PermissionDto
                {
                    CanView = true,
                    CanEdit = true,
                    CanDownload = true,
                    CanAnnotate = true,
                    CanDelete = true,
                    CanShare = true
                };
            }

            // If user is document owner, they have all permissions
            if (await IsDocumentOwnerAsync(documentId, userId))
            {
                return new PermissionDto
                {
                    CanView = true,
                    CanEdit = true,
                    CanDownload = true,
                    CanAnnotate = true,
                    CanDelete = true,
                    CanShare = true
                };
            }

            // Check explicit permissions
            var document = await _context.Documents
                .Include(d => d.AccessibilityList)
                .ThenInclude(a => a.AccessibilityListItems)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document?.AccessibilityList?.AccessibilityListItems == null)
                return new PermissionDto();

            var permission = document.AccessibilityList.AccessibilityListItems
                .FirstOrDefault(a => a.UserId == userId);

            if (permission == null)
                return new PermissionDto();

            return new PermissionDto
            {
                CanView = permission.CanView,
                CanEdit = permission.CanEdit,
                CanDownload = permission.CanDownload,
                CanAnnotate = permission.CanAnnotate,
                CanDelete = permission.CanDelete,
                CanShare = permission.CanShare
            };
        }

        public async Task<bool> IsDocumentOwnerAsync(Guid documentId, Guid userId)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId);

            return document?.CreatedById == userId;
        }

        private async Task<bool> IsUserAdminAsync(Guid userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            return user?.Role == "Admin";
        }
    }
}
