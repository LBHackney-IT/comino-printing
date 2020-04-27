using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boundary.UseCaseInterfaces;
using comino_print_api.Responses;
using UseCases.GatewayInterfaces;

namespace UseCases
{
    public class QueryDocumentsAndSendToNotify : IQueryDocumentsAndSendToNotify
    {
        private readonly ILocalDatabaseGateway _localDatabaseGateway;
        private readonly IS3Gateway _s3Gateway;
        private readonly IGovNotifyGateway _govNotifyGateway;

        public QueryDocumentsAndSendToNotify(ILocalDatabaseGateway localDatabaseGateway, IS3Gateway s3Gateway, IGovNotifyGateway govNotifyGateway)
        {
            _localDatabaseGateway = localDatabaseGateway;
            _s3Gateway = s3Gateway;
            _govNotifyGateway = govNotifyGateway;
        }
        public async Task Execute()
        {
            var documentResponse = await _localDatabaseGateway.GetDocumentsThatAreReadyForGovNotify();
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

            documentResponses.ForEach(async document => {
                var pdfBytesResponse = await _s3Gateway.GetPdfDocumentAsByteArray(document.DocNo);
                // log this response

                var uniqueRef = "uniqueRefHere"; // a unique reference to this postage attempt - should include timestamp?
                // update document in localdb with this uniqueRef

                var govNotifyResponse = _govNotifyGateway.SendPdfDocumentForPostage(pdfBytesResponse, uniqueRef);
                // log this response
            });
        }
    }
}