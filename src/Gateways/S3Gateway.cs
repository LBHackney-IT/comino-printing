using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Usecases.Domain;
using UseCases.GatewayInterfaces;

namespace Gateways
{
    public class S3Gateway : IS3Gateway
    {
        private readonly IAmazonS3 _amazonS3;

        public S3Gateway(IAmazonS3 amazonS3)
        {
            _amazonS3 = amazonS3;
        }

        public async Task<Response> SavePdfDocument(string documentId)
        {
            var bucketName = Environment.GetEnvironmentVariable("GENERATED_PDF_BUCKET_NAME");

            var tempFilePath =  $"/tmp/{documentId}.pdf";

            var date = DateTime.Parse(documentId);
            var s3FilePath = $"{date:yyyy}/{date:MM}/{date:dd}/{documentId}.pdf";

            await _amazonS3.PutObjectAsync(new PutObjectRequest
            {
                Key = s3FilePath,
                ContentType = "application/pdf",
                FilePath = tempFilePath,
                BucketName = bucketName
            });

            File.Delete(tempFilePath);
            Console.WriteLine($"> S3Gateway SavePdfDocument documentId: {documentId} to bucket: {bucketName} and path {s3FilePath}");
            return new Response { Success = true };
        }

        public string GeneratePdfUrl(string docId)
        {
            var bucketName = Environment.GetEnvironmentVariable("GENERATED_PDF_BUCKET_NAME");
            var date = DateTime.Parse(docId);
            var s3FilePath = $"{date:yyyy}/{date:MM}/{date:dd}/{docId}.pdf";

            var pdfUrl= _amazonS3.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = s3FilePath,
                Expires = DateTime.Now.AddMinutes(5)
            });

            return pdfUrl;
        }

        public async Task<byte[]> GetPdfDocumentAsByteArray(string cominoDocumentNumber)
        {
            var objReq = new GetObjectRequest
            {
                BucketName = Environment.GetEnvironmentVariable("GENERATED_PDF_BUCKET_NAME"),
                Key = $"{cominoDocumentNumber}.pdf",
            };

            var objResp = await _amazonS3.GetObjectAsync(objReq);
            var ms = new MemoryStream();
            await objResp.ResponseStream.CopyToAsync(ms);

            return ms.ToArray();
        }
    }
}