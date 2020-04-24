using System;
using System.Collections.Generic;
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
        private string _convertHtmlToPdfEnvVar;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _mockAmazonS3 = new Mock<IAmazonS3>();
            _subject = new S3Gateway(_mockAmazonS3.Object);

            _currentBucketName = Environment.GetEnvironmentVariable("GENERATED_PDF_BUCKET_NAME");
            _convertHtmlToPdfEnvVar = Environment.GetEnvironmentVariable("CONVERT_HTML_TO_PDF");
            Environment.SetEnvironmentVariable("CONVERT_HTML_TO_PDF", "true");
            _testBucketName = _fixture.Create<string>();
            Environment.SetEnvironmentVariable("GENERATED_PDF_BUCKET_NAME", _testBucketName);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("CONVERT_HTML_TO_PDF", _convertHtmlToPdfEnvVar);
            Environment.SetEnvironmentVariable("GENERATED_PDF_BUCKET_NAME", _currentBucketName);
        }

        [Test]
        public async Task SavePdfDocument_SendsTheFileToS3()
        {
            var documentId = _fixture.Create<string>();
            await _subject.SavePdfDocument(documentId);

            var expectedFilePath = $"/tmp/{documentId}.pdf";
            File.Create(expectedFilePath);

            var expectedPutRequest = new PutObjectRequest
            {
                BucketName = _testBucketName,
                Key = documentId,
                ContentType = "application/pdf",
                FilePath = expectedFilePath
            };
            _mockAmazonS3.Verify(x => x.PutObjectAsync(It.Is(Match(expectedPutRequest)), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task SavePdfDocument_RemovesFileFromTmp()
        {
            var documentId = _fixture.Create<string>();

            var expectedFilePath = $"/tmp/{documentId}.pdf";
            File.Create(expectedFilePath);

            var expectedPutRequest = new PutObjectRequest
            {
                BucketName = _testBucketName,
                Key = documentId,
                ContentType = "application/pdf",
                FilePath = expectedFilePath
            };
            _mockAmazonS3.Setup(x => x.PutObjectAsync(It.Is(Match(expectedPutRequest)), It.IsAny<CancellationToken>()));

            await _subject.SavePdfDocument(documentId);
            File.Exists(expectedFilePath).Should().BeFalse();
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