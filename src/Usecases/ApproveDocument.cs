using System.Threading.Tasks;
using Usecases.Enums;
using UseCases.GatewayInterfaces;
using Usecases.Interfaces;

namespace UseCases
{
    public class ApproveDocument : IApproveDocument
    {
        private readonly ILocalDatabaseGateway _localDatabaseGateway;

        public ApproveDocument(ILocalDatabaseGateway localDatabaseGateway)
        {
            _localDatabaseGateway = localDatabaseGateway;
        }

        public async Task Execute(string id)
        {
            await _localDatabaseGateway.UpdateStatus(id, LetterStatusEnum.ReadyForGovNotify);
        }
    }
}