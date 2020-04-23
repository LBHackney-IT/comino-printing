using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Usecases.Enums;
using UseCases.GatewayInterfaces;
using Usecases.Interfaces;

namespace UseCases
{
    public class UpdateDocumentState : IUpdateDocumentState
    {
        private ILocalDatabaseGateway _localDatabaseGateway;

        public UpdateDocumentState(ILocalDatabaseGateway localDatabaseGateway)
        {
            _localDatabaseGateway = localDatabaseGateway;
        }
        
        public async Task Execute(string id, string status)
        {
            var documentTimestamp = id;
            
            var requestedStatus = Enum.Parse<LetterStatusEnum>(status);
            
            await _localDatabaseGateway.UpdateStatus(documentTimestamp, requestedStatus);
        }
    }
}