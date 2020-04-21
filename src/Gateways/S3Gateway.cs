using System;
using UseCases.GatewayInterfaces;

namespace Gateways
{
    public class S3Gateway : IS3Gateway
    {
        public string SavePdfDocument(string documentId, byte[] filename)
        {
            Console.WriteLine($"> S3Gateway SavePdfDocument documentId: {documentId}, filename: {filename}");

            return "[s3 response here]";
        }
    }
}