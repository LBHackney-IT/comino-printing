using System;
using Usecases.GatewayInterfaces;

namespace Usecases
{
    public class GetDocumentsIds
    {
        private ICominoGateway _cominoGateway;
        
        public GetDocumentsIds(ICominoGateway cominoGateway)
        {
            _cominoGateway = cominoGateway;
        }

        public void Execute()
        {
            _cominoGateway.GetDocumentsAfterStartDate(DateTime.Now.AddMinutes(-1));
        }
    }
}