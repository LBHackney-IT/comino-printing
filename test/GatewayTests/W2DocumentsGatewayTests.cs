using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Gateways;
using Moq.Protected;
using Newtonsoft.Json;

namespace GatewayTests
{
    public class W2DocumentsGatewayTest
    {
        private W2DocumentsGateway _subject;
        private HttpClient _httpClientMock;
        private IFixture _fixture;
        private string _url;
        private string _currentEnv;
        private Mock<HttpMessageHandler> _messageHandler;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _url = "http://test-domain-name/";
            _currentEnv = Environment.GetEnvironmentVariable("W2_DOCUMENT_BASE_URL");
            Environment.SetEnvironmentVariable("W2_DOCUMENT_BASE_URL", _url);
            _messageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            var httpClient = new HttpClient (_messageHandler.Object){
                BaseAddress = new Uri(_url),
            };
            _subject = new W2DocumentsGateway(httpClient);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("W2_DOCUMENT_BASE_URL", _currentEnv);
        }

        [Test]
        public async Task GetHtmlDocumentCorrectlyCallsTheDocumentsGateway()
        {
            var documentId = _fixture.Create<string>();
            var expectedUrl = $"{_url}/hncomino/documents/{documentId}/view";

            SetUpMessageHandlerToReturnHtml();

            await _subject.GetHtmlDocument(documentId);

            _messageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get
                        && req.RequestUri == new Uri(expectedUrl)
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Test]
        public async Task GetHtmlDocumentReturnsReceivedHtml()
        {
            var documentId = _fixture.Create<string>();
            var expectedHtml = $"<html><p>{_fixture.Create<string>()}</p></html>";

            SetUpMessageHandlerToReturnHtml(expectedHtml);

            var result = await _subject.GetHtmlDocument(documentId);
            result.Should().Be(expectedHtml);
        }

        private void SetUpMessageHandlerToReturnHtml(string expectedHtml = "<p>expected html</p>")
        {
            var stubbedResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedHtml)
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
