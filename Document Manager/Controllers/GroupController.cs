using Document_Manager.DTOs;
using Document_Manager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Document_Manager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;
        private readonly ILogger<GroupController> _logger;

        public GroupController(IGroupService groupService, ILogger<GroupController> logger)
        {
            _groupService = groupService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetAllGroups()
        {
            try
            {
                var groups = await _groupService.GetAllGroupsAsync();
                return Ok(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all groups");
                return StatusCode(500, "An error occurred while retrieving groups");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GroupDto>> GetGroup(int id)
        {
            try
            {
                var group = await _groupService.GetGroupByIdAsync(id);
                if (group == null)
                {
                    return NotFound($"Group with ID {id} not found");
                }

                // Only allow admins or members to see group details
                if (!User.IsInRole("Admin") && !await IsUserGroupMember(id))
                {
                    return Forbid("You do not have permission to view this group");
                }

                return Ok(group);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving group {GroupId}", id);
                return StatusCode(500, "An error occurred while retrieving the group");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GroupDto>> CreateGroup(GroupCreateDto groupDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdGroup = await _groupService.CreateGroupAsync(groupDto);
                return CreatedAtAction(nameof(GetGroup), new { id = createdGroup.Id }, createdGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group");
                return StatusCode(500, "An error occurred while creating the group");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GroupDto>> UpdateGroup(int id, GroupUpdateDto groupDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedGroup = await _groupService.UpdateGroupAsync(id, groupDto);
                if (updatedGroup == null)
                {
                    return NotFound($"Group with ID {id} not found");
                }

                return Ok(updatedGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group {GroupId}", id);
                return StatusCode(500, "An error occurred while updating the group");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            try
            {
                var result = await _groupService.DeleteGroupAsync(id);
                if (!result)
                {
                    return NotFound($"Group with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group {GroupId}", id);
                return StatusCode(500, "An error occurred while deleting the group");
            }
        }

        [HttpGet("{id}/members")]
        public async Task<ActionResult<IEnumerable<DocumentUserSummaryDto>>> GetGroupMembers(int id)
        {
            try
            {
                // Only allow admins or members to see group members
                if (!User.IsInRole("Admin") && !await IsUserGroupMember(id))
                {
                    return Forbid("You do not have permission to view this group's members");
                }

                var members = await _groupService.GetGroupMembersAsync(id);
                return Ok(members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving members for group {GroupId}", id);
                return StatusCode(500, "An error occurred while retrieving group members");
            }
        }

        [HttpPost("{id}/members/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddMemberToGroup(int id, Guid userId)
        {
            try
            {
                var result = await _groupService.AddMemberToGroupAsync(id, userId);
                if (!result)
                {
                    return NotFound("Group or user not found");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {UserId} to group {GroupId}", userId, id);
                return StatusCode(500, "An error occurred while adding member to group");
            }
        }

        [HttpDelete("{id}/members/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveMemberFromGroup(int id, Guid userId)
        {
            try
            {
                // Allow users to remove themselves from groups
                var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!User.IsInRole("Admin") && 
                    (string.IsNullOrEmpty(currentUserIdString) || 
                    !Guid.TryParse(currentUserIdString, out var currentUserId) ||
                    currentUserId != userId))
                {
                    return Forbid("You do not have permission to remove this member from the group");
                }

                var result = await _groupService.RemoveMemberFromGroupAsync(id, userId);
                if (!result)
                {
                    return NotFound("Group membership not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from group {GroupId}", userId, id);
                return StatusCode(500, "An error occurred while removing member from group");
            }
        }

        // Helper method to check if the current user is a member of the specified group
        private async Task<bool> IsUserGroupMember(int groupId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return false;
            }

            var members = await _groupService.GetGroupMembersAsync(groupId);
            return members.Any(m => m.UserId == userId);
        }
    }
}
