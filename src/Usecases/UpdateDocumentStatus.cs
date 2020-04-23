using System;
using System.Threading.Tasks;
using Usecases.Enums;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class UpdateDocumentStatus
    {
        private ILocalDatabaseGateway _localDatabaseGateway;

        public UpdateDocumentStatus(ILocalDatabaseGateway localDatabaseGateway)
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