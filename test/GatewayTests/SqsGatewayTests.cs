using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using AutoFixture;
using FluentAssertions;
using Gateways;
using Moq;
using NUnit.Framework;

namespace GatewayTests
{
    public class SqsGatewayTest
    {
        private SqsGateway _subject;
        private Mock<AmazonSQSClient> _sqsClient;

        [SetUp]
        public void Setup()
        {
            _sqsClient = new Mock<AmazonSQSClient>();
            _subject = new SqsGateway(_sqsClient.Object);
        }

        [Ignore("To Fix")]
        [Test]
        public void AddDocumentIdsToQueueCallsTheSendMessageAsyncMethodOnTheSqsClient()
        {
            const string documentId = "123456";

            _subject.AddDocumentIdsToQueue(documentId);

            _sqsClient
                .Verify(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Test]
        [Ignore("To Fix")]
        public void AddDocumentIdsToQueueReturnsAMessageResponseContainingTheCorrectId()
        {
            const string documentId = "123456";

            _sqsClient.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (SendMessageRequest request, CancellationToken ct) =>
                        new SendMessageResponse
                        {
                            MD5OfMessageBody = $"{request.MessageBody}"
                        });

            var response = _subject.AddDocumentIdsToQueue(documentId);

            response.MD5OfMessageBody.Should().BeEquivalentTo(documentId);
        }
    }
}
