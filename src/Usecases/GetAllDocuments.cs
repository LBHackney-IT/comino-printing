using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boundary.UseCaseInterfaces;
using comino_print_api.Models;
using comino_print_api.Responses;
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
        public async Task<GetAllDocumentsResponse> Execute()
        {
           var documentResponse = await _localDatabaseGateway.GetAllRecords();
           var documentResponses = documentResponse.Select(record => new DocumentResponse
           {
               Id = record.SavedAt,
               DocNo = record.DocumentId,
               Sender = record.DocumentCreator,
               Created = record.SavedAt,
               Status = record.Status.ToString(),
               LetterType = record.LetterType,
               DocumentType = record.DocumentType,
               Logs = record.Log?.Select(x => new Dictionary<string, string>
               {
                   {"date", x.Key},
                   {"message", x.Value}
               }).ToList()
           }).ToList();

           return new GetAllDocumentsResponse
           {
               Documents = documentResponses
           };
        }
    }
}