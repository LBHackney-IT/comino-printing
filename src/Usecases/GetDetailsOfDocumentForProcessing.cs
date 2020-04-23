using System.Threading.Tasks;
using Usecases.Domain;
using Usecases.Enums;
using UseCases.GatewayInterfaces;
using Usecases.Interfaces;

namespace UseCases
{
    public class GetDetailsOfDocumentForProcessing : IGetDetailsOfDocumentForProcessing
    {
        private readonly ILocalDatabaseGateway _databaseGateway;

        public GetDetailsOfDocumentForProcessing(ILocalDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public async Task<DocumentDetails> Execute(string timeStamp)
        {
            var response = await _databaseGateway.RetrieveDocumentAndSetStatusToProcessing(timeStamp);
            return response;
        }
    }
}