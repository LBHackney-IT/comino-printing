using System.Threading.Tasks;
using Usecases.Enums;
using UseCases.GatewayInterfaces;
using Usecases.Interfaces;

namespace Usecases
{
    public class CancelDocument : ICancelDocument
    {
        private ILocalDatabaseGateway _localDatabaseGateway;

        public CancelDocument(ILocalDatabaseGateway localDatabaseGateway)
        {
            _localDatabaseGateway = localDatabaseGateway;
        }
        
        public async Task Execute(string id)
        {
            await _localDatabaseGateway.UpdateStatus(id, LetterStatusEnum.Cancelled);
        }
    }
}