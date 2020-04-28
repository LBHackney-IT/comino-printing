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

        public QueryDocumentsAndSendToNotify(ILocalDatabaseGateway localDatabaseGateway, IS3Gateway s3Gateway,
            IGovNotifyGateway govNotifyGateway, IDbLogger logger)
        {
            _localDatabaseGateway = localDatabaseGateway;
            _s3Gateway = s3Gateway;
            _govNotifyGateway = govNotifyGateway;
            _logger = logger;
        }
        public async Task Execute()
        {
            var documents = await _localDatabaseGateway.GetDocumentsThatAreReadyForGovNotify();

            documents.ForEach(async document => {
                var pdfBytesResponse = await _s3Gateway.GetPdfDocumentAsByteArray(document.CominoDocumentNumber);
                // log this response


                var govNotifyResponse = _govNotifyGateway.SendPdfDocumentForPostage(pdfBytesResponse, document.CominoDocumentNumber);
                if (govNotifyResponse.Success)
                {
                    await _localDatabaseGateway.UpdateStatus(document.Id, LetterStatusEnum.SentToGovNotify);
                    await _localDatabaseGateway.SaveSendNotificationId(document.Id, govNotifyResponse.NotificationId);
                    await _logger.LogMessage(document.Id,
                        $"Sent to Gov Notify. Gov Notify Notification Id {document.GovNotifyNotificationId}");
                }
                else
                {
                    await _localDatabaseGateway.UpdateStatus(document.Id, LetterStatusEnum.GovNotifySendError);
                    await _logger.LogMessage(document.Id, $"Error Sending to GovNotify: {govNotifyResponse.Error}");
                }
            });
        }
    }
}