using System.Collections.Generic;
using Usecases.Enums;

namespace Usecases.Domain
{
    public class DocumentDetails
    {
        public string CominoDocumentNumber { get; set; }
        public string DocumentCreator { get; set; }
        public string Id { get; set; }
        public string Date { get; set; }
        public string LetterType { get; set; }
        public string DocumentType { get; set; }
        public LetterStatusEnum Status { get; set; }
        public Dictionary<string, string> Log { get; set; }
        public string GovNotifyNotificationId { get; set; }
    }
}
