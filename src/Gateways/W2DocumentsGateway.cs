using System;
// using System.IO;
// using System.Net;
// using System.Threading;
// using System.Text;
using UseCases.GatewayInterfaces;

namespace Gateways
{
    public class W2DocumentsGateway : IW2DocumentsGateway
    {

        public string GetHtmlDocument(string documentId)
        {
            Console.WriteLine($"> W2DocumentsGateway GetHtmlDocument documentId: {documentId}");

            return "[html content here]";
        }
    }
}
