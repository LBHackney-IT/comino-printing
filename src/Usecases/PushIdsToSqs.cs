using System;
using System.Collections.Generic;
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

        public void Execute(List<string> documentIds)
        {
            _sqsGateway.AddDocumentIdsToQueue(documentIds);
        }
    }
}