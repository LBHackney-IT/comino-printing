using System.Collections.Generic;
using System.Threading.Tasks;
using Usecases.Domain;
using Usecases.Enums;

namespace UseCases.GatewayInterfaces
{
    public interface ILocalDatabaseGateway
    {
        Task SaveDocument(DocumentDetails newDocument);

        Task<DocumentDetails> GetRecordByTimeStamp(string id);

        Task<UpdateStatusResponse> UpdateStatus(string id, LetterStatusEnum newStatus);
        Task<DocumentDetails> RetrieveDocumentAndSetStatusToProcessing(string id);

        Task<List<DocumentDetails>> GetAllRecords(int limit, string cursor);

        Task<List<DocumentDetails>> GetDocumentsThatAreReadyForGovNotify();
        Task<List<DocumentDetails>> GetLettersWaitingForGovNotify();
        Task SaveSendNotificationId(string documentId, string sentNotificationId);
    }
}
