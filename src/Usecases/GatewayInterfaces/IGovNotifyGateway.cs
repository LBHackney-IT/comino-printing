using Notify.Models.Responses;
using System.Threading.Tasks;

namespace UseCases.GatewayInterfaces
{
    public interface IGovNotifyGateway
    {
        Task<LetterNotificationResponse> SendPdfDocumentForPostage(byte[] pdfBytes, string uniqueRef);
    }
}