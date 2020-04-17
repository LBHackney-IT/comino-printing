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
        private Mock<AmazonSQSClient> _client;
        private Fixture _fixture;
        
        [SetUp]
        public void Setup()
        {
            _client = new Mock<AmazonSQSClient>();
            _subject = new SqsGateway(_client.Object);
            _fixture = new Fixture();
        }
        
        [Test]
        public void AddDocumentIdsToQueueCallsTheSqsClient()
        {
            var documentIds = _fixture.CreateMany<string>().ToList();

            _subject.AddDocumentIdsToQueue(documentIds);
            _client
                .Verify(x => x.SendMessageBatchAsync(It.IsAny<SendMessageBatchRequest>(), CancellationToken.None), 
                    Times.Once);
        }
    }
}