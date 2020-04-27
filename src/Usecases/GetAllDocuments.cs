using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boundary.UseCaseInterfaces;
using comino_print_api.Models;
using comino_print_api.Responses;
using Usecases;
using UseCases.GatewayInterfaces;

namespace UseCases
{
    public class GetAllDocuments : IGetAllDocuments
    {
        private readonly ILocalDatabaseGateway _localDatabaseGateway;

        public GetAllDocuments(ILocalDatabaseGateway localDatabaseGateway)
        {
            _localDatabaseGateway = localDatabaseGateway;
        }
        public async Task<GetAllDocumentsResponse> Execute(string limit, string cursor)
        {
            var parsedLimit = Convert.ToInt32(limit);
            var documentResponse = await _localDatabaseGateway.GetAllRecords(parsedLimit, cursor);
            var documentResponses = documentResponse.Select(record => new DocumentResponse
            {
                Id = record.Id,
                DocNo = record.CominoDocumentNumber,
                Sender = record.DocumentCreator,
                Created = record.Id,
                Status = record.Status.PrettierStatusName(),
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