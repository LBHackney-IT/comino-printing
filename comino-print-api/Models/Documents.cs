namespace comino_print_api.Models
{
    public class Document
    {
        public string DocumentId { get; set; }

        public string DocumentCreator { get; set; }
        
        public string CreatedAt { get; set; }
        
        // public string PrintedAt { get; set; }
        // public string Status { get; set; }
        // public string StatusUpdatedAt { get; set; }
    }
}