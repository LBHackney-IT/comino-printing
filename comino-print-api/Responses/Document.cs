using System.Collections.Generic;
using Usecases.Domain;

namespace comino_print_api.Responses
{
    public class Document
    {
        public int Id { get; set; }
        public string DocumentId { get; set; }
        public string DocumentCreator { get; set; }
        public string CreatedAt { get; set; }
        public string PrintedAt { get; set; }
        public string Status { get; set; }
        public string StatusUpdatedAt { get; set; }
        public string LetterType { get; set; }
        public string DocumentType { get; set; }
        public List<DocumentLog> Logs { get; set; }
    }
}