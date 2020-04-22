using System.Collections.Generic;
using System.Threading.Tasks;
using comino_print_api.Models;
using comino_print_api.Repositories;

// using JsonApiDotNetCore.Models;
// using Document = JsonApiDotNetCore.Models.Document;

namespace comino_print_api.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        
        public DocumentService(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }
        
        public async Task<IEnumerable<Document>> ListAsync()
        {
            return await _documentRepository.ListAsync();
        }
    }
}