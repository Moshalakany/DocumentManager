namespace Document_Manager.Models
{
    public class DocumentAccessibilityList
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        
        // Navigation property for associated document
        public Document? Document { get; set; }
        
        //Navigation properties
        public List<DocumentAccessibilityListItem>? AccessibilityListItems { get; set; } = new List<DocumentAccessibilityListItem>();
    }
}
