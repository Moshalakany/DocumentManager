namespace Document_Manager.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        //Navigation properties
        public List<Document>? Documents { get; set; } = new List<Document>();
        
        // Add color for UI display
        public string? Color { get; set; }
        
        // Add category for organization
        public string? Category { get; set; }
        
        // Add created by reference
        public Guid? CreatedById { get; set; }
        public User? CreatedBy { get; set; }
        
        // Add timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
