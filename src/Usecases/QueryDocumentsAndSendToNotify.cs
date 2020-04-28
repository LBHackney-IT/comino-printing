using System;
using System.Threading.Tasks;
using Boundary.UseCaseInterfaces;
using Usecases.Enums;
using Usecases.GatewayInterfaces;
using UseCases.GatewayInterfaces;

namespace UseCases
{
    public class QueryDocumentsAndSendToNotify : IQueryDocumentsAndSendToNotify
    {
        private readonly ILocalDatabaseGateway _localDatabaseGateway;
        private readonly IS3Gateway _s3Gateway;
        private readonly IGovNotifyGateway _govNotifyGateway;
        private readonly IDbLogger _logger;
        private readonly ICominoGateway _cominoGateway;

        public QueryDocumentsAndSendToNotify(ILocalDatabaseGateway localDatabaseGateway, IS3Gateway s3Gateway,
            IGovNotifyGateway govNotifyGateway, ICominoGateway cominoGateway, IDbLogger logger)
        {
            _localDatabaseGateway = localDatabaseGateway;
            _s3Gateway = s3Gateway;
            _govNotifyGateway = govNotifyGateway;
            _cominoGateway = cominoGateway;
            _logger = logger;
        }
        public async Task Execute()
        {
            var documents = await _localDatabaseGateway.GetDocumentsThatAreReadyForGovNotify();

            documents.ForEach(async document => {
                var pdfBytesResponse = await _s3Gateway.GetPdfDocumentAsByteArray(
                    document.Id, document.CominoDocumentNumber
                );

                var sentStatus = _cominoGateway.GetDocumentSentStatus(document.Id);
                if (sentStatus.Printed)
                {
                    await _localDatabaseGateway.UpdateStatus(document.Id, LetterStatusEnum.PrintedManually);
                    await _logger.LogMessage(document.Id,
                        $"Not sent to GovNotify. Document already printed, printed at {sentStatus.PrintedAt}");
                    return;
                }

                var govNotifyResponse = _govNotifyGateway.SendPdfDocumentForPostage(pdfBytesResponse, document.CominoDocumentNumber);
                if (govNotifyResponse.Success)
                {
                    await _localDatabaseGateway.UpdateStatus(document.Id, LetterStatusEnum.SentToGovNotify);
                    await _localDatabaseGateway.SaveSendNotificationId(document.Id, govNotifyResponse.NotificationId);
                    await _logger.LogMessage(document.Id,
                        $"Sent to Gov Notify. Gov Notify Notification Id {document.GovNotifyNotificationId}.");
                    _cominoGateway.MarkDocumentAsSent(document.CominoDocumentNumber);
                    await _logger.LogMessage(document.Id, "Removed from batch print queue and print date set in comino");
                }
                else
                {
                    await _localDatabaseGateway.UpdateStatus(document.Id, LetterStatusEnum.FailedToSend);
                    await _logger.LogMessage(document.Id, $"Error Sending to GovNotify: {govNotifyResponse.Error}");
                }
            });
        }
    }
}
