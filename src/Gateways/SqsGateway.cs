using System;
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
        private readonly AmazonSQSClient _sqsClient;
        private string _queueUrl;

        public SqsGateway(AmazonSQSClient sqsClient)
        {
            _sqsClient = sqsClient;
            _queueUrl = Environment.GetEnvironmentVariable("SQS_URL");
        }

        public SendMessageResponse AddDocumentIdsToQueue(string documentId)
        {
            var sqsMessageRequest = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = $"{documentId}"
            };
            
            return _sqsClient.SendMessageAsync(sqsMessageRequest).Result;
        }
    }
}