using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using UseCases.GatewayInterfaces;

namespace Gateways
{
    public class SqsGateway : ISqsGateway
    {
        private readonly AmazonSQSClient _client;

        public SqsGateway(AmazonSQSClient client)
        {
            _client = client;
        }

        public Task<SendMessageBatchResponse> AddDocumentIdsToQueue(List<string> documentsIds)
        {
            var messageEntries = new List<SendMessageBatchRequestEntry>();
            
            var i = 1;
            
            foreach (var docId in documentsIds)
            {
                messageEntries.Add(new SendMessageBatchRequestEntry($"message{i}",$"{docId}"));
                i++;
            }

            var sendMessageBatchRequest = new SendMessageBatchRequest
            {
                Entries = messageEntries, 
                QueueUrl = ""
            };
            
            return _client.SendMessageBatchAsync(sendMessageBatchRequest, CancellationToken.None);
        }
    }
}