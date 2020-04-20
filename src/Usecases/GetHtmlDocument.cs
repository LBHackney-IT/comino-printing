using System;
using System.Collections.Generic;
using AwsDotnetCsharp.UsecaseInterfaces;
using UseCases.GatewayInterfaces;

namespace UseCases
{
    public class GetHtmlDocument : IGetHtmlDocument
    {
        private readonly IW2DocumentsGateway _w2DocumentsGateway;

        public GetHtmlDocument(IW2DocumentsGateway w2DocumentsGateway)
        {
            _w2DocumentsGateway = w2DocumentsGateway;
        }

        public string Execute(string documentId)
        {
            Console.WriteLine($"> GetHtmlDocument usecase documentId: {documentId}");
            return _w2DocumentsGateway.GetHtmlDocument(documentId);
        }
    }
}
