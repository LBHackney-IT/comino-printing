using System;
using AwsDotnetCsharp.UsecaseInterfaces;
using UseCases.GatewayInterfaces;

namespace UseCases
{
    public class SavePdfToS3 : ISavePdfToS3
    {
        private readonly IS3Gateway _s3Gateway;

        public SavePdfToS3(IS3Gateway s3Gateway)
        {
            _s3Gateway = s3Gateway;
        }

        public string Execute(string documentId, byte[] filename)
        {
            Console.WriteLine($"> SavePdfToS3 usecase documentID {documentId}, filename ${filename}");

            return _s3Gateway.SavePdfDocument(documentId, filename);
        }
    }
}