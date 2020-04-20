using System;
using System.Collections.Generic;
using Amazon.SQS.Model;
using AwsDotnetCsharp.UsecaseInterfaces;
using UseCases.GatewayInterfaces;

namespace UseCases
{
    public class PushIdsToSqs : IPushIdsToSqs
    {
        private readonly ISqsGateway _sqsGateway;
        
        public PushIdsToSqs(ISqsGateway sqsGateway)
        {
            _sqsGateway = sqsGateway;
        }

        public List<SendMessageResponse> Execute(List<string> documentIds)
        {
            var messageResponses = new List<SendMessageResponse>();
            
            foreach (var docId in documentIds)
            {
                messageResponses.Add(_sqsGateway.AddDocumentIdsToQueue(docId));
            }

            return messageResponses;
        }
    }
}