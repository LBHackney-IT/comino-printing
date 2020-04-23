using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using comino_print_api.Responses;
using comino_print_api.UseCaseInterFaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core;

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