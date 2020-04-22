using System.Collections;
using System.Threading.Tasks;
using Usecases.Domain;
using Usecases.Enums;

namespace UseCases.GatewayInterfaces
{
    public interface ILocalDatabaseGateway
    {
        Task<string> SaveDocument(DocumentDetails newDocument);

        Task<DocumentDetails> GetRecordByTimeStamp(string currentTimestamp);

        Task UpdateStatus(string savedDocumentSavedAt, LetterStatusEnum newStatus);
        Task<DocumentDetails> RetrieveDocumentAndSetStatusToProcessing(string savedDocumentSavedAt);
    }
}