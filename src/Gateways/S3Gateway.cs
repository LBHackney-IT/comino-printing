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
            var filePath = $"./{documentId}.pdf";

            await _amazonS3.PutObjectAsync(new PutObjectRequest
            {
                Key = documentId,
                ContentType = "application/pdf",
                FilePath = filePath,
                BucketName = bucketName
            });

            File.Delete(filePath);
            Console.WriteLine($"> S3Gateway SavePdfDocument documentId: {documentId} to bucket: {bucketName}");
            return new Response { Success = true };
        }
    }
}