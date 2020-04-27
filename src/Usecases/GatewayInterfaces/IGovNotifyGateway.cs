using Notify.Models.Responses;
using System.Threading.Tasks;
using Usecases.Domain;

namespace UseCases.GatewayInterfaces
{
    public interface IGovNotifyGateway
    {
        Task<LetterNotificationResponse> SendPdfDocumentForPostage(byte[] pdfBytes, string uniqueRef);
        GovNotifyResponse GetStatusForLetter(string documentId, string govNotifyNotificationId);
    }
}