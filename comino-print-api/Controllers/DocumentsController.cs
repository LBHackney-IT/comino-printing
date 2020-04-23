using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Boundary.UseCaseInterfaces;
using comino_print_api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core;
using UseCases;
using Usecases.Enums;

namespace comino_print_api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class DocumentsController : Controller
    {
        private readonly IGetAllDocuments _getAllDocuments;
        private IUpdateDocuments _updateDocuments;

        public DocumentsController(IGetAllDocuments getAllDocuments, IUpdateDocuments updateDocuments)
        {
            _getAllDocuments = getAllDocuments;
            _updateDocuments = updateDocuments;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] EndIdParameter endIdParameter)
        {
            var documents = await _getAllDocuments.Execute(endIdParameter);
            return Ok(documents);
        }
        
        public void GetById([FromQuery] string id)
        {
            //Get s3 pdf Url from somewhere
            //use Response.Redirect("")to redirect to the pdf in s3;
        }
        
        [HttpPut]
        public void UpdateDocumentState([FromBody] string id, string status)
        {
            _updateDocuments.Execute(id, status);
        }
    }
}