using System.Collections.Generic;
using System.Threading.Tasks;
using Usecases.Domain;
using Usecases.Enums;

namespace UseCases.GatewayInterfaces
{
    public interface ILocalDatabaseGateway
    {
        Task<string> SaveDocument(DocumentDetails newDocument);

        Task<DocumentDetails> GetRecordByTimeStamp(string currentTimestamp);

        Task<UpdateStatusResponse> UpdateStatus(string savedDocumentSavedAt, LetterStatusEnum newStatus);
        Task<DocumentDetails> RetrieveDocumentAndSetStatusToProcessing(string savedDocumentSavedAt);

        Task<List<DocumentDetails>> GetAllRecords(int limit, string cursor);

        Task SetStatusToReadyForNotify(string putRequestId);

        Task<List<DocumentDetails>> GetDocumentsThatAreReadyForGovNotify();
        Task<List<DocumentDetails>> GetLettersWaitingForGovNotify();
    }
}