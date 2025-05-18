namespace Document_Manager.DTOs
{
    public class DocumentResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? FileType { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public UserSummaryDto CreatedBy { get; set; }
        public IEnumerable<string> Tags { get; set; } = new List<string>();
        public int Version { get; set; }
    }

    public class UserSummaryDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string? Email { get; set; }
    }
}
