using Document_Manager.Data;
using Document_Manager.DTOs;
using Document_Manager.Models;
using Document_Manager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Document_Manager.Services
{
    public class GroupService : IGroupService
    {
        private readonly AppDbContextSQL _context;

        public GroupService(AppDbContextSQL context)
        {
            _context = context;
        }

        public async Task<List<GroupDto>> GetAllGroupsAsync()
        {
            var groups = await _context.GroupPermissions
                .Include(g => g.GroupPermissionUsers)
                .ToListAsync();

            return groups.Select(MapGroupToDto).ToList();
        }

        public async Task<GroupDto> GetGroupByIdAsync(int id)
        {
            var group = await _context.GroupPermissions
                .Include(g => g.GroupPermissionUsers)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return null;

            return MapGroupToDto(group);
        }

        public async Task<GroupDto> CreateGroupAsync(GroupCreateDto groupDto)
        {
            var group = new GroupPermission
            {
                Name = groupDto.Name,
                Description = groupDto.Description,
                CanCreate = groupDto.CanCreate,
                CanRead = groupDto.CanRead,
                CanUpdate = groupDto.CanUpdate,
                CanDelete = groupDto.CanDelete,
                IsAdmin = groupDto.IsAdmin
            };

            await _context.GroupPermissions.AddAsync(group);
            await _context.SaveChangesAsync();

            return MapGroupToDto(group);
        }

        public async Task<GroupDto> UpdateGroupAsync(int id, GroupUpdateDto groupDto)
        {
            var group = await _context.GroupPermissions
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return null;

            group.Name = groupDto.Name;
            group.Description = groupDto.Description;
            group.CanCreate = groupDto.CanCreate;
            group.CanRead = groupDto.CanRead;
            group.CanUpdate = groupDto.CanUpdate;
            group.CanDelete = groupDto.CanDelete;
            group.IsAdmin = groupDto.IsAdmin;

            await _context.SaveChangesAsync();

            return await GetGroupByIdAsync(id);
        }

        public async Task<bool> DeleteGroupAsync(int id)
        {
            var group = await _context.GroupPermissions
                .Include(g => g.GroupPermissionUsers)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return false;

            // Remove associated members first
            if (group.GroupPermissionUsers?.Any() == true)
            {
                _context.GroupPermissionsUsers.RemoveRange(group.GroupPermissionUsers);
            }

            _context.GroupPermissions.Remove(group);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddMemberToGroupAsync(int groupId, Guid userId)
        {
            var group = await _context.GroupPermissions
                .Include(g => g.GroupPermissionUsers)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return false;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Check if user is already a member of the group
            if (group.GroupPermissionUsers.Any(gpu => gpu.UserId == userId))
                return true; // User is already a member, consider this a success

            var groupUser = new GroupPermissionsUser
            {
                GroupPermissionId = groupId,
                UserId = userId
            };

            await _context.GroupPermissionsUsers.AddAsync(groupUser);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveMemberFromGroupAsync(int groupId, Guid userId)
        {
            var groupUser = await _context.GroupPermissionsUsers
                .FirstOrDefaultAsync(gpu => gpu.GroupPermissionId == groupId && gpu.UserId == userId);

            if (groupUser == null)
                return false;

            _context.GroupPermissionsUsers.Remove(groupUser);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<UserDocumentUserSummaryDto>> GetGroupMembersAsync(int groupId)
        {
            var members = await _context.GroupPermissionsUsers
                .Include(gpu => gpu.User)
                .Where(gpu => gpu.GroupPermissionId == groupId)
                .ToListAsync();

            return members
                .Where(m => m.User != null)
                .Select(m => new UserDocumentUserSummaryDto
                {
                    UserId = m.UserId,
                    Username = m.User.Username,
                    Email = m.User.Email,
                    Role = m.User.Role
                })
                .ToList();
        }

        // Helper method to map GroupPermission to GroupDto
        private GroupDto MapGroupToDto(GroupPermission group)
        {
            return new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                CanCreate = group.CanCreate,
                CanRead = group.CanRead,
                CanUpdate = group.CanUpdate,
                CanDelete = group.CanDelete,
                IsAdmin = group.IsAdmin,
                MemberCount = group.GroupPermissionUsers?.Count ?? 0
            };
        }

        Task<List<DocumentUserSummaryDto>> IGroupService.GetGroupMembersAsync(int groupId)
        {
            throw new NotImplementedException();
        }
    }
}
