using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
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
            _sqsClient = new Mock<AmazonSQSClient>(RegionEndpoint.EUWest2);
            _subject = new SqsGateway(_sqsClient.Object);
        }
        
        [Test]
        [Ignore("To Fix")]
        public void AddDocumentIdsToQueueCallsTheSendMessageAsyncMethodOnTheSqsClient()
        {
            const string documentId = "123456";

            _subject.AddDocumentIdsToQueue(documentId);

            _sqsClient
                .Verify(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Test]
        public void AddDocumentIdsToQueueReturnsAMessageResponseContainingTheCorrectId()
        {
            const string documentId = "123456";

            var expectedHash = CreateMd5Hash(documentId);

            _sqsClient.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (SendMessageRequest request, CancellationToken ct) =>
                        new SendMessageResponse
                        {
                            MD5OfMessageBody = $"{CreateMd5Hash(request.MessageBody)}"
                        });

            var response = _subject.AddDocumentIdsToQueue(documentId);

            response.MD5OfMessageBody.Should().BeEquivalentTo(expectedHash);
        }
        
        static string CreateMd5Hash(string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (var i = data.Length - 1; i >= 0; i--)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
