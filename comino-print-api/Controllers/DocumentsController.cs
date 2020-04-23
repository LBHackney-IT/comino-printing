using System.Threading.Tasks;
using Boundary.UseCaseInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace comino_print_api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class DocumentsController : Controller
    {
        private readonly IGetAllDocuments _getAllDocuments;

        public DocumentsController(IGetAllDocuments getAllDocuments)
        {
            _getAllDocuments = getAllDocuments;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var documents = await _getAllDocuments.Execute();
            return Ok(documents);
        }
    }
}