using Document_Manager.DTOs;
using Document_Manager.Models;

namespace Document_Manager.Services.Interfaces
{
    public interface IGroupService
    {
        Task<List<GroupDto>> GetAllGroupsAsync();
        Task<GroupDto> GetGroupByIdAsync(int id);
        Task<GroupDto> CreateGroupAsync(GroupCreateDto groupDto);
        Task<GroupDto> UpdateGroupAsync(int id, GroupUpdateDto groupDto);
        Task<bool> DeleteGroupAsync(int id);
        
        Task<bool> AddMemberToGroupAsync(int groupId, Guid userId);
        Task<bool> RemoveMemberFromGroupAsync(int groupId, Guid userId);
        Task<List<DocumentUserSummaryDto>> GetGroupMembersAsync(int groupId);
    }
}
