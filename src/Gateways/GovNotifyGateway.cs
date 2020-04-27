using Notify.Models.Responses;
using System.Threading.Tasks;
using Notify.Interfaces;
using Usecases.Domain;
using Usecases.Enums;
using UseCases.GatewayInterfaces;

namespace Gateways
{
    public class GovNotifyGateway : IGovNotifyGateway
    {
        private readonly INotificationClient _govNotifyClient;

        public GovNotifyGateway(INotificationClient govNotifyClient)
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

        public GovNotifyResponse GetStatusForLetter(string documentId, string govNotifyNotificationId)
        {
            var response = _govNotifyClient.GetNotificationById(govNotifyNotificationId);

            return new GovNotifyResponse
            {
                Status = GetStatus(response.status),
                SentAt = response.completedAt
            };
        }

        private static LetterStatusEnum GetStatus(string govNotifyStatus)
        {
            switch (govNotifyStatus)
            {
                case "pending-virus-check":
                    return LetterStatusEnum.GovNotifyPendingVirusCheck;
                case "virus-scan-failed":
                    return LetterStatusEnum.GovNotifyVirusScanFailed;
                case "validation-failed":
                    return LetterStatusEnum.GovNotifyValidationFailed;
                default:
                    return LetterStatusEnum.LetterSent;
            }
        }
    }
}
