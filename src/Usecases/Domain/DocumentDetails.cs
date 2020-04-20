namespace Usecases.Domain
{
    public class DocumentDetails
    {
        public string DocumentId { get; set; }
        public string DocumentCreator { get; set; }
        public string SavedAt { get; set; }

        public string LetterType { get; set; }
        public string DocumentType { get; set; }
    }
}