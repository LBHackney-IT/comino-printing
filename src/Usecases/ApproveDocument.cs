using System.Threading.Tasks;
using UseCases.GatewayInterfaces;
using Usecases.Interfaces;

namespace UseCases
{
    public class ApproveDocument : IApproveDocument
    {
        private ILocalDatabaseGateway _localDatabaseGateway;

        public ApproveDocument(ILocalDatabaseGateway localDatabaseGateway)
        {
            _localDatabaseGateway = localDatabaseGateway;
        }
        
        public async Task Execute(string id)
        {
            await _localDatabaseGateway.SetStatusToReadyForNotify(id);
        }
    }
}