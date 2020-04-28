using Usecases.Domain;

namespace UseCases.GatewayInterfaces
{
    public interface IGovNotifyGateway
    {
        GovNotifySendResponse SendPdfDocumentForPostage(byte[] pdfBytes, string uniqueRef);
        GovNotifyStatusResponse GetStatusForLetter(string documentId, string govNotifyNotificationId);
    }
}