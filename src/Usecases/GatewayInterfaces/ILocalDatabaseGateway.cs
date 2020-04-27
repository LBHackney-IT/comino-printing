using System.Collections.Generic;
using System.Threading.Tasks;
using Usecases.Domain;
using Usecases.Enums;

namespace UseCases.GatewayInterfaces
{
    public interface ILocalDatabaseGateway
    {
        Task<string> SaveDocument(DocumentDetails newDocument);

        Task<DocumentDetails> GetRecordByTimeStamp(string id);

        Task<UpdateStatusResponse> UpdateStatus(string id, LetterStatusEnum newStatus);
        Task<DocumentDetails> RetrieveDocumentAndSetStatusToProcessing(string id);

        Task<List<DocumentDetails>> GetAllRecords(int limit, string cursor);

        Task SetStatusToReadyForNotify(string id);

        Task<List<DocumentDetails>> GetDocumentsThatAreReadyForGovNotify();
        Task<List<DocumentDetails>> GetLettersWaitingForGovNotify();
    }
}