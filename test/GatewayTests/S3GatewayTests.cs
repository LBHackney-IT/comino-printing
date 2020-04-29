using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using AutoFixture;
using FluentAssertions;
using Gateways;
using Moq;
using NUnit.Framework;

namespace GatewayTests
{
    public class S3GatewayTests
    {
        private Mock<IAmazonS3> _mockAmazonS3;
        private S3Gateway _subject;
        private string _currentBucketName;
        private Fixture _fixture;
        private string _testBucketName;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _mockAmazonS3 = new Mock<IAmazonS3>();
            _subject = new S3Gateway(_mockAmazonS3.Object);

            _currentBucketName = Environment.GetEnvironmentVariable("GENERATED_PDF_BUCKET_NAME");
            _testBucketName = _fixture.Create<string>();
            Environment.SetEnvironmentVariable("GENERATED_PDF_BUCKET_NAME", _testBucketName);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GENERATED_PDF_BUCKET_NAME", _currentBucketName);
        }

        [Test]
        public async Task SavePdfDocument_SendsTheFileToS3()
        {
            var date = new DateTime(2020, 02, 25, 12, 45, 27);
            var documentId = date.ToString("O");
            await _subject.SavePdfDocument(documentId);

            var expectedFilePath = $"/tmp/{documentId}.pdf";
            File.Create(expectedFilePath);

            var expectedPutRequest = new PutObjectRequest
            {
                BucketName = _testBucketName,
                Key = $"2020/02/25/{documentId}.pdf",
                ContentType = "application/pdf",
                FilePath = expectedFilePath
            };
            _mockAmazonS3.Verify(x =>
                x.PutObjectAsync(It.Is(Match(expectedPutRequest)), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task SavePdfDocument_RemovesFileFromTmp()
        {
            var date = new DateTime(2018, 03, 02, 12, 45, 27);
            var documentId = date.ToString("O");

            var expectedFilePath = $"/tmp/{documentId}.pdf";
            File.Create(expectedFilePath);

            var expectedPutRequest = new PutObjectRequest
            {
                BucketName = _testBucketName,
                Key = $"2018/03/02/{documentId}.pdf",
                ContentType = "application/pdf",
                FilePath = expectedFilePath
            };
            _mockAmazonS3.Setup(x => x.PutObjectAsync(It.Is(Match(expectedPutRequest)), It.IsAny<CancellationToken>()));

            await _subject.SavePdfDocument(documentId);
            File.Exists(expectedFilePath).Should().BeFalse();
        }

        [Test]
        public void GeneratePdfUrl_CallsGetPreSignedURLOnTheS3Client()
        {
            var date = new DateTime(2020, 02, 25, 12, 45, 27);
            var documentId = date.ToString("O");

            _subject.GeneratePdfUrl(documentId);

            _mockAmazonS3.Verify(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()));
        }

        [Test]
        public async Task GetPdfDocumentAsByteArray_ThrowsIfNoPdfFound()
        {
            var date = new DateTime(2020, 02, 25, 12, 45, 27);
            var documentId = date.ToString("O");
            var s3Key = $"2020/02/25/{documentId}.pdf";

            Func<Task> act = async () => {
                await  _subject.GetPdfDocumentAsByteArray(documentId);
            };

            await act.Should().ThrowAsync<Exception>();
        }

        [Test]
        public async Task GetPdfDocumentAsByteArray_ReturnsPdfByteArrayIfPdfFound()
        {
            var date = new DateTime(2020, 02, 25, 12, 45, 27);
            var documentId = date.ToString("O");
            var s3Key = $"2020/02/25/{documentId}.pdf";

            // prep mock S3 client to return a successful response
            var getObjectResponseMock = new GetObjectResponse() {
                Expires = _fixture.Create<DateTime>(),
                ResponseStream = new MemoryStream()
            };

            _mockAmazonS3.Setup(mockObj => mockObj.GetObjectAsync(
                _testBucketName,
                s3Key,
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(getObjectResponseMock);

            var expected = new byte[]{};
            var received = await _subject.GetPdfDocumentAsByteArray(documentId);

            received.Should().BeEquivalentTo(expected);

            _mockAmazonS3.Verify(x =>
                x.GetObjectAsync(_testBucketName, s3Key, It.IsAny<CancellationToken>()));
        }

        private static Expression<Func<PutObjectRequest, bool>> Match(PutObjectRequest expectedPutRequest)
        {
            return p => p.BucketName == expectedPutRequest.BucketName
                && p.Key == expectedPutRequest.Key
                && p.ContentType == expectedPutRequest.ContentType
                && p.FilePath == expectedPutRequest.FilePath;
        }
    }
}
