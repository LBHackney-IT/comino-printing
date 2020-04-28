using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Boundary.UseCaseInterfaces;
using UseCases.GatewayInterfaces;
using Usecases.Interfaces;

namespace UseCases
{
    public class GetHtmlDocument : IGetHtmlDocument
    {
        private readonly IW2DocumentsGateway _w2DocumentsGateway;
        private readonly IS3Gateway _s3Gateway;

        public GetHtmlDocument(IW2DocumentsGateway w2DocumentsGateway, IS3Gateway s3Gateway)
        {
            _w2DocumentsGateway = w2DocumentsGateway;
            _s3Gateway = s3Gateway;
        }

        public async Task<string> Execute(string documentId)
        {
            Console.WriteLine($"> GetHtmlDocument usecase documentId: {documentId}");
            var html = await _w2DocumentsGateway.GetHtmlDocument(documentId);

            await File.WriteAllTextAsync($"/tmp/{documentId}.html", html);

            //TODO Remove later - for troubleshooting PDF Generation
            await _s3Gateway.SaveHtmlDocument(documentId);

            return html;
        }
    }
}
