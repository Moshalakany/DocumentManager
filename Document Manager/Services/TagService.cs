using Document_Manager.Data;
using Document_Manager.DTOs;
using Document_Manager.Models;
using Document_Manager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Document_Manager.Services
{
    public class TagService : ITagService
    {
        private readonly AppDbContextSQL _context;

        public TagService(AppDbContextSQL context)
        {
            _context = context;
        }

        public async Task<Tag> CreateTagAsync(TagCreateDto tagDto, Guid userId)
        {
            // Check if tag with same name already exists
            var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagDto.Name);
            if (existingTag != null)
            {
                return existingTag;
            }

            var tag = new Tag
            {
                Id = Guid.NewGuid(),
                Name = tagDto.Name,
                Color = tagDto.Color,
                Category = tagDto.Category,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();

            return tag;
        }

        public async Task<Tag?> GetTagByIdAsync(Guid id)
        {
            return await _context.Tags
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await _context.Tags
                .Include(t => t.CreatedBy)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<List<Tag>> GetTagsByUserIdAsync(Guid userId)
        {
            return await _context.Tags
                .Where(t => t.CreatedById == userId)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Tag?> UpdateTagAsync(Guid id, TagUpdateDto tagDto)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return null;
            }

            // Update properties
            tag.Name = tagDto.Name ?? tag.Name;
            tag.Color = tagDto.Color ?? tag.Color;
            tag.Category = tagDto.Category ?? tag.Category;

            _context.Tags.Update(tag);
            await _context.SaveChangesAsync();

            return tag;
        }

        public async Task<bool> DeleteTagAsync(Guid id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return false;
            }

            // Remove tag from all documents
            var documents = await _context.Documents
                .Include(d => d.Tags)
                .Where(d => d.Tags.Any(t => t.Id == id))
                .ToListAsync();

            foreach (var document in documents)
            {
                var tagToRemove = document.Tags.FirstOrDefault(t => t.Id == id);
                if (tagToRemove != null)
                {
                    document.Tags.Remove(tagToRemove);
                }
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Tag>> GetTagsForDocumentAsync(Guid documentId)
        {
            var document = await _context.Documents
                .Include(d => d.Tags)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            return document?.Tags?.ToList() ?? new List<Tag>();
        }

        public async Task<bool> AddTagToDocumentAsync(Guid documentId, Guid tagId)
        {
            var document = await _context.Documents
                .Include(d => d.Tags)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                return false;
            }

            var tag = await _context.Tags.FindAsync(tagId);
            if (tag == null)
            {
                return false;
            }

            // Check if tag is already added to the document
            if (document.Tags.Any(t => t.Id == tagId))
            {
                return true; // Tag already exists, return true
            }

            document.Tags.Add(tag);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveTagFromDocumentAsync(Guid documentId, Guid tagId)
        {
            var document = await _context.Documents
                .Include(d => d.Tags)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                return false;
            }

            var tag = document.Tags.FirstOrDefault(t => t.Id == tagId);
            if (tag == null)
            {
                return false;
            }

            document.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Tag>> UpdateDocumentTagsAsync(Guid documentId, List<Guid> tagIds)
        {
            var document = await _context.Documents
                .Include(d => d.Tags)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                throw new KeyNotFoundException($"Document with ID {documentId} not found.");
            }

            // Clear existing tags
            document.Tags.Clear();

            // Add new tags
            if (tagIds.Any())
            {
                var tagsToAdd = await _context.Tags
                    .Where(t => tagIds.Contains(t.Id))
                    .ToListAsync();

                foreach (var tag in tagsToAdd)
                {
                    document.Tags.Add(tag);
                }
            }

            await _context.SaveChangesAsync();
            return document.Tags.ToList();
        }
    }
}
