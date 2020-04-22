using System.Threading.Tasks;
using comino_print_api.Models;
using comino_print_api.UseCaseInterFaces;
using UseCases.GatewayInterfaces;

namespace UseCases
{
    public class GetAllDocuments : IGetAllDocuments
    {
        private ILocalDatabaseGateway _localDatabaseGateway;

        public GetAllDocuments(ILocalDatabaseGateway localDatabaseGateway)
        {
            _localDatabaseGateway = localDatabaseGateway;
        }
        public Task<GetAllDocumentsResponse> Execute()
        {
            // Get all from the database and map to class
            return null;
        }
    }
}