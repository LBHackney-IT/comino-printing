using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Boundary.UseCaseInterfaces;
using comino_print_api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core;
using UseCases;

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
        public async Task<IActionResult> GetAllAsync([FromQuery] EndIdParameter endIdParameter)
        {
            var endId = endIdParameter;
            var documents = await _getAllDocuments.Execute(endId);
            return Ok(documents);
        }
    }
}