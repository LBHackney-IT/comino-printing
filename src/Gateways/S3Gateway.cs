using System;
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

        //TODO Remove later - for troubleshooting PDF Generation
        public async Task<Response> SaveHtmlDocument(string documentId)
        {
            var bucketName = Environment.GetEnvironmentVariable("GENERATED_PDF_BUCKET_NAME");

            var tempFilePath =  $"/tmp/{documentId}.html";

            var date = DateTime.Parse(documentId);
            var s3FilePath = $"{date:yyyy}/{date:MM}/{date:dd}/{documentId}.html";

            await _amazonS3.PutObjectAsync(new PutObjectRequest
            {
                Key = s3FilePath,
                ContentType = "text/html",
                FilePath = tempFilePath,
                BucketName = bucketName
            });

            File.Delete(tempFilePath);
            Console.WriteLine($"> S3Gateway HTML documentId: {documentId} to bucket: {bucketName} and path {s3FilePath}");
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

        public async Task<byte[]> GetPdfDocumentAsByteArray(string documentId, string cominoDocumentNumber)
        {
            var date = DateTime.Parse(documentId);
            var s3Key = $"{date:yyyy}/{date:MM}/{date:dd}/{cominoDocumentNumber}.pdf";
            var bucketName = Environment.GetEnvironmentVariable("GENERATED_PDF_BUCKET_NAME");

            GetObjectResponse response = await _amazonS3.GetObjectAsync(bucketName, s3Key);

            using (Stream responseStream = response.ResponseStream)
            {
                return ReadStream(responseStream);
            }
        }

        private static byte[] ReadStream(Stream responseStream)
        {
            byte[] buffer = new byte[16 * 1024];

            using (MemoryStream ms = new MemoryStream())
            {
                int read;

                while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
    }
}
