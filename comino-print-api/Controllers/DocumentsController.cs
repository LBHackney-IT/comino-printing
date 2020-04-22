using System.Collections.Generic;
using System.Threading.Tasks;
using comino_print_api.Models;
using comino_print_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace comino_print_api.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class DocumentsController : Controller
    {
        private readonly IDocumentService _documentService;
        
        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;   
        }

        [HttpGet]
        public async Task<IEnumerable<Document>> GetAllAsync()
        {
            var documents = await _documentService.ListAsync();
            return documents;
        }
    }
}