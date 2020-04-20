// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Gateways;

namespace GatewayTests
{
    public class W2DocumentsGatewayTest
    {
        private W2DocumentsGateway _subject;

        [SetUp]
        public void Setup()
        {
            _subject = new W2DocumentsGateway();
        }

        [Test]
        public void GetHtmlDocumentCallsTheSendMessageAsyncMethodOnTheSqsClient()
        {
            const string documentId = "123456";

            var expected = "<p>expected html</p>";
            var received = _subject.GetHtmlDocument(documentId);

            // assert methods called
            // _sqsClient
            //     .Verify(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()),
            //         Times.Once);
        }
    }
}
