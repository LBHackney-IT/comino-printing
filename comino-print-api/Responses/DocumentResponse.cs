using System.Collections.Generic;

namespace comino_print_api.Responses
{
    public class DocumentResponse
    {
        public string Id { get; set; }
        public string DocNo { get; set; }
        public string Sender { get; set; }
        public string Created { get; set; }
        public string PrintedAt { get; set; }
        public string Status { get; set; }
        public string StatusUpdatedAt { get; set; }
        public string LetterType { get; set; }
        public string DocumentType { get; set; }
        public List<Dictionary<string, string>> Logs { get; set; }
    }
}