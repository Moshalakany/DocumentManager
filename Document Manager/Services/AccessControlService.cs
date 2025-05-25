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
        private readonly int _maxRetries = 3;

        public AccessControlService(AppDbContextSQL context)
        {
            _context = context;
        }

        public async Task<bool> AssignDocumentPermissionToUserAsync(Guid documentId, Guid userId, PermissionDto permissions)
        {
            int retryCount = 0;
            bool succeeded = false;

            while (!succeeded && retryCount < _maxRetries)
            {
                try
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    var document = await _context.Documents
                        .Include(d => d.AccessibilityList)
                        .ThenInclude(a => a.AccessibilityListItems)
                        .FirstOrDefaultAsync(d => d.Id == documentId);

                    if (document == null)
                        return false;

                    if (document.AccessibilityList == null)
                    {
                        document.AccessibilityList = new DocumentAccessibilityList
                        {
                            Id = Guid.NewGuid(),
                            DocumentId = documentId,
                            AccessibilityListItems = new List<DocumentAccessibilityListItem>()
                        };
                    }

                    var existingPermission = document.AccessibilityList.AccessibilityListItems?
                        .FirstOrDefault(a => a.UserId == userId);

                    if (existingPermission != null)
                    {
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
                    await transaction.CommitAsync();
                    
                    succeeded = true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    _context.ChangeTracker.Clear();
                    
                    retryCount++;
                    if (retryCount >= _maxRetries)
                        throw; 
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return succeeded;
        }

        public async Task<bool> AssignDocumentPermissionToGroupAsync(Guid documentId, int groupId, PermissionDto permissions)
        {
            int retryCount = 0;
            bool succeeded = false;

            while (!succeeded && retryCount < _maxRetries)
            {
                try
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
                    // Get fresh group data
                    var group = await _context.GroupPermissions
                        .AsNoTracking()  // Use AsNoTracking to avoid interference with tracked entities
                        .Include(g => g.GroupPermissionUsers)
                        .FirstOrDefaultAsync(g => g.Id == groupId);

                    if (group == null)
                        return false;

                    // Document existence check
                    var documentExists = await _context.Documents.AnyAsync(d => d.Id == documentId);
                    if (!documentExists)
                        return false;

                    bool allSucceeded = true;
                    foreach (var userGroup in group.GroupPermissionUsers)
                    {
                        // For each user, we assign permissions in a separate operation but within the same transaction
                        var result = await AssignDocumentPermissionToUserWithoutTransactionAsync(
                            documentId, userGroup.UserId, permissions);
                        if (!result) allSucceeded = false;
                    }

                    if (allSucceeded)
                    {
                        await transaction.CommitAsync();
                        succeeded = true;
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Clear the change tracker to remove stale entities
                    _context.ChangeTracker.Clear();
                    
                    retryCount++;
                    if (retryCount >= _maxRetries)
                        throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return succeeded;
        }

        // Helper method for use within a transaction
        private async Task<bool> AssignDocumentPermissionToUserWithoutTransactionAsync(
            Guid documentId, Guid userId, PermissionDto permissions)
        {
            // Get fresh document data
            var document = await _context.Documents
                .Include(d => d.AccessibilityList)
                .ThenInclude(a => a.AccessibilityListItems)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                return false;

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

        public async Task<bool> RemoveDocumentPermissionForUserAsync(Guid documentId, Guid userId)
        {
            int retryCount = 0;
            bool succeeded = false;

            while (!succeeded && retryCount < _maxRetries)
            {
                try
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
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
                    await transaction.CommitAsync();
                    
                    succeeded = true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Clear the change tracker to remove stale entities
                    _context.ChangeTracker.Clear();
                    
                    retryCount++;
                    if (retryCount >= _maxRetries)
                        throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return succeeded;
        }

        public async Task<bool> RemoveDocumentPermissionForGroupAsync(Guid documentId, int groupId)
        {
            int retryCount = 0;
            bool succeeded = false;

            while (!succeeded && retryCount < _maxRetries)
            {
                try
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
                    // Get fresh group data
                    var group = await _context.GroupPermissions
                        .AsNoTracking()  // Use AsNoTracking to avoid interference with tracked entities
                        .Include(g => g.GroupPermissionUsers)
                        .FirstOrDefaultAsync(g => g.Id == groupId);

                    if (group?.GroupPermissionUsers == null)
                        return false;

                    bool allSucceeded = true;
                    // For each user in the group, remove the permissions
                    foreach (var userGroup in group.GroupPermissionUsers)
                    {
                        var result = await RemoveDocumentPermissionForUserWithoutTransactionAsync(
                            documentId, userGroup.UserId);
                        if (!result) allSucceeded = false;
                    }

                    if (allSucceeded)
                    {
                        await transaction.CommitAsync();
                        succeeded = true;
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Clear the change tracker to remove stale entities
                    _context.ChangeTracker.Clear();
                    
                    retryCount++;
                    if (retryCount >= _maxRetries)
                        throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return succeeded;
        }

        // Helper method for use within a transaction
        private async Task<bool> RemoveDocumentPermissionForUserWithoutTransactionAsync(Guid documentId, Guid userId)
        {
            // Get fresh document data
            var document = await _context.Documents
                .Include(d => d.AccessibilityList)
                .ThenInclude(a => a.AccessibilityListItems)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document?.AccessibilityList?.AccessibilityListItems == null)
                return false;

            var permission = document.AccessibilityList.AccessibilityListItems
                .FirstOrDefault(a => a.UserId == userId);

            if (permission == null)
                return true; // Permission already doesn't exist, consider this a success

            document.AccessibilityList.AccessibilityListItems.Remove(permission);
            await _context.SaveChangesAsync();
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
                            : new PermissionDto { CanView = true } 
                });
            }

            return result;
        }

        public async Task<bool> CanUserViewDocumentAsync(Guid documentId, Guid userId)
        {
            if (await IsUserAdminAsync(userId))
                return true;

            if (await IsDocumentOwnerAsync(documentId, userId))
                return true;

            var permissions = await GetUserDocumentPermissionsAsync(documentId, userId);
            return permissions.CanView;
        }

        public async Task<bool> CanUserEditDocumentAsync(Guid documentId, Guid userId)
        {
            if (await IsUserAdminAsync(userId))
                return true;

            if (await IsDocumentOwnerAsync(documentId, userId))
                return true;

            var permissions = await GetUserDocumentPermissionsAsync(documentId, userId);
            return permissions.CanEdit;
        }

        public async Task<bool> CanUserDownloadDocumentAsync(Guid documentId, Guid userId)
        {
            if (await IsUserAdminAsync(userId))
                return true;

            if (await IsDocumentOwnerAsync(documentId, userId))
                return true;

            var permissions = await GetUserDocumentPermissionsAsync(documentId, userId);
            return permissions.CanDownload;
        }

        public async Task<bool> CanUserDeleteDocumentAsync(Guid documentId, Guid userId)
        {
            if (await IsUserAdminAsync(userId))
                return true;

            if (await IsDocumentOwnerAsync(documentId, userId))
                return true;

            var permissions = await GetUserDocumentPermissionsAsync(documentId, userId);
            return permissions.CanDelete;
        }

        public async Task<bool> CanUserShareDocumentAsync(Guid documentId, Guid userId)
        {
            if (await IsUserAdminAsync(userId))
                return true;

            if (await IsDocumentOwnerAsync(documentId, userId))
                return true;

            var permissions = await GetUserDocumentPermissionsAsync(documentId, userId);
            return permissions.CanShare;
        }

        public async Task<bool> CanUserAnnotateDocumentAsync(Guid documentId, Guid userId)
        {
            if (await IsUserAdminAsync(userId))
                return true;

            if (await IsDocumentOwnerAsync(documentId, userId))
                return true;

            var permissions = await GetUserDocumentPermissionsAsync(documentId, userId);
            return permissions.CanAnnotate;
        }

        public async Task<PermissionDto> GetUserDocumentPermissionsAsync(Guid documentId, Guid userId)
        {
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
