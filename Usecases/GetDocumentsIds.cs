using System;
using UseCases.GatewayInterfaces;

namespace UseCases
{
    public class GetDocumentsIds
    {
        private readonly ICominoGateway _cominoGateway;
        private ISqsGateway _sqsGateway;

        public GetDocumentsIds(ICominoGateway cominoGateway, ISqsGateway sqsGateway)
        {
            _cominoGateway = cominoGateway;
            _sqsGateway = sqsGateway;
        }

        public void Execute()
        {
            var receivedDocumentIds = _cominoGateway.GetDocumentsAfterStartDate(DateTime.Now.AddMinutes(-1));
            _sqsGateway.AddDocumentIdsToQueue(receivedDocumentIds);
        }
    }
}
