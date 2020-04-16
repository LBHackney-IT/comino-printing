using System;
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
    }
}