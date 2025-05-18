namespace Document_Manager.Models
{
    public class FileValidation
    {
        public Guid Id { get; set; }
        public string FileExtension { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long MaxSizeInBytes { get; set; } = 104857600; // Default 100MB
        public bool IsAllowed { get; set; } = true;
        
        // For OCR and content processing
        public bool SupportsOcr { get; set; } = false;
        public bool SupportsPreview { get; set; } = false;
    }
}
