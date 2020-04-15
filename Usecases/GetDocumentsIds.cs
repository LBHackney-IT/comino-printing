using System;
using UseCases.GatewayInterfaces;

namespace UseCases
{
    public class GetDocumentsIds
    {
        private readonly ICominoGateway _cominoGateway;

        public GetDocumentsIds(ICominoGateway cominoGateway)
        {
            _cominoGateway = cominoGateway;
        }

        public void Execute()
        {
            var receivedDocumentIds = _cominoGateway.GetDocumentsAfterStartDate(DateTime.Now.AddMinutes(-1));
        }
    }
}
