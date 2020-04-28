using Notify.Exceptions;
using Notify.Interfaces;
using Notify.Models.Responses;
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

        public GovNotifySendResponse SendPdfDocumentForPostage(byte[] pdfBytes, string uniqueRef)
        {
            LetterNotificationResponse response;
            try
            {
                response =  _govNotifyClient.SendPrecompiledLetter(
                    uniqueRef,
                    pdfBytes
                );
            }
            catch (NotifyClientException e)
            {
                return new GovNotifySendResponse
                {
                    Error = e.Message,
                    Success = false
                };
            }

            return new GovNotifySendResponse
            {
                NotificationId = response.id,
                Success = true,
            };
        }

        public GovNotifyStatusResponse GetStatusForLetter(string documentId, string govNotifyNotificationId)
        {
            var response = _govNotifyClient.GetNotificationById(govNotifyNotificationId);

            return new GovNotifyStatusResponse
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
