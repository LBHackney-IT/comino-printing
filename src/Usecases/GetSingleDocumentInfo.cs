using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using comino_print_api.Models;
using comino_print_api.Responses;
using UseCases.GatewayInterfaces;
using Usecases.Interfaces;

namespace Usecases
{
    public class GetSingleDocumentInfo : IGetSingleDocumentInfo
    {
        private ILocalDatabaseGateway _localDatabaseGateway;

        public GetSingleDocumentInfo(ILocalDatabaseGateway localDatabaseGateway)
        {
            _localDatabaseGateway = localDatabaseGateway;
        }
        public async Task<SingleDocumentResponse> Execute(string id)
        {
            var singleDocInfo = await _localDatabaseGateway.GetRecordByTimeStamp(id);
            
            var singleDocumentResponse = new DocumentResponse
            {
                Id = singleDocInfo.Id,
                DocNo = singleDocInfo.CominoDocumentNumber,
                Sender = singleDocInfo.DocumentCreator,
                Created = singleDocInfo.Id,
                Status = singleDocInfo.Status.PrettierStatusName(),
                LetterType = singleDocInfo.LetterType,
                DocumentType = singleDocInfo.DocumentType,
                Logs = singleDocInfo.Log?.Select(x => new Dictionary<string, string>
                {
                    {"date", x.Key},
                    {"message", x.Value}
                }).ToList()
            };
            
            return new SingleDocumentResponse
            {
                Document = singleDocumentResponse
            };
        }
    }
}