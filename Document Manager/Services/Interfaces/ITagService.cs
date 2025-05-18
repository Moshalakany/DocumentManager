using Document_Manager.DTOs;
using Document_Manager.Models;

namespace Document_Manager.Services.Interfaces
{
    public interface ITagService
    {
        // Tag CRUD operations
        Task<Tag> CreateTagAsync(TagCreateDto tagDto, Guid userId);
        Task<Tag?> GetTagByIdAsync(Guid id);
        Task<List<Tag>> GetAllTagsAsync();
        Task<List<Tag>> GetTagsByUserIdAsync(Guid userId);
        Task<Tag?> UpdateTagAsync(Guid id, TagUpdateDto tagDto);
        Task<bool> DeleteTagAsync(Guid id);
        
        // Document tagging operations
        Task<List<Tag>> GetTagsForDocumentAsync(Guid documentId);
        Task<bool> AddTagToDocumentAsync(Guid documentId, Guid tagId);
        Task<bool> RemoveTagFromDocumentAsync(Guid documentId, Guid tagId);
        Task<List<Tag>> UpdateDocumentTagsAsync(Guid documentId, List<Guid> tagIds);
    }
}
