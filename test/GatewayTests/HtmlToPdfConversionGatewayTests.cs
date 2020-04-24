using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Gateways;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace GatewayTests
{
    public class HtmlToPdfConversionGatewayTests
    {
        private HtmlToPdfConversionGateway _subject;
        private Fixture _fixture;
        private string _url;
        private string _currentEnv;
        private Mock<HttpMessageHandler> _messageHandler;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _url = "http://test-domain-name/";
            _currentEnv = Environment.GetEnvironmentVariable("HTML_TO_PDF_CONVERSION_ENDPOINT");
            Environment.SetEnvironmentVariable("HTML_TO_PDF_CONVERSION_ENDPOINT", _url);
            _messageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            var httpClient = new HttpClient (_messageHandler.Object){
                BaseAddress = new Uri(_url),
            };
            _subject = new HtmlToPdfConversionGateway(httpClient);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("HTML_TO_PDF_CONVERSION_ENDPOINT", _currentEnv);
        }

        [Test]
        public async Task GetHtmlDocumentCorrectlyCallsTheDocumentsGateway()
        {
            var documentId = _fixture.Create<string>();
            var html = _fixture.Create<string>();
            var expectedUrl = _url;

            SetUpMessageHandlerToReturnPdf();

            await _subject.Convert(html, documentId);

            _messageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post
                        && req.RequestUri == new Uri(expectedUrl)
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Test]
        public async Task GetHtmlDocumentReturnsReceivedPdfBytes()
        {
            var documentId = _fixture.Create<string>();
            var html = _fixture.Create<string>();
            var expectedPdfBytes = _fixture.Create<byte[]>();

            SetUpMessageHandlerToReturnPdf(expectedPdfBytes);

            var result = await _subject.Convert(html, documentId);
            result.Should().Equal(expectedPdfBytes);
        }

        private void SetUpMessageHandlerToReturnPdf(byte[] expectedPdfBytes = null)
        {
            if (expectedPdfBytes == null)
            {
                expectedPdfBytes = _fixture.Create<byte[]>();
            }
            var stubbedResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(expectedPdfBytes)
            };

            _messageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(stubbedResponse)
                .Verifiable();
        }
    }
}