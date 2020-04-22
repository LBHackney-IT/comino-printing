using System.Collections.Generic;
using comino_print_api.Responses;

namespace comino_print_api.Models
{
    public class GetAllDocumentsResponse
    {
        public List<DocumentResponse> Documents { get; set; }
    }
}