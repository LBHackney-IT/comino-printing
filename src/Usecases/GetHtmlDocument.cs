using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Boundary.UseCaseInterfaces;
using UseCases.GatewayInterfaces;
using Usecases.Interfaces;

namespace UseCases
{
    public class GetHtmlDocument : IGetHtmlDocument
    {
        private readonly IW2DocumentsGateway _w2DocumentsGateway;

        public GetHtmlDocument(IW2DocumentsGateway w2DocumentsGateway)
        {
            _w2DocumentsGateway = w2DocumentsGateway;
        }

        public async Task<string> Execute(string documentId)
        {
            Console.WriteLine($"> GetHtmlDocument usecase documentId: {documentId}");
            var html = await _w2DocumentsGateway.GetHtmlDocument(documentId);
            return html;
        }
    }
}
