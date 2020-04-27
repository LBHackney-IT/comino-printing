using Notify.Client;
using Notify.Models.Responses;
using System.Threading.Tasks;
using UseCases.GatewayInterfaces;

namespace Gateways
{
    public class GovNotifyGateway : IGovNotifyGateway //, IDbLogger
    {
        private readonly NotificationClient _govNotifyClient;

        public GovNotifyGateway(NotificationClient govNotifyClient)
        {
            _govNotifyClient = govNotifyClient;
        }

        public async Task<LetterNotificationResponse> SendPdfDocumentForPostage(byte[] pdfBytes, string uniqueRef)
        {
            return _govNotifyClient.SendPrecompiledLetter(
                uniqueRef,
                pdfBytes
                // postage (optional, either "first" or "second", defaults to "second")
            );
        }
    }
}
