using Notify.Models.Responses;
using System.Threading.Tasks;
using Usecases.Enums;

namespace UseCases.GatewayInterfaces
{
    public interface IGovNotifyGateway
    {
        Task<LetterNotificationResponse> SendPdfDocumentForPostage(byte[] pdfBytes, string uniqueRef);
        GovNotifyResponseEnum GetStatusForLetter(string documentId, string govNotifyNotificationId);
    }
}