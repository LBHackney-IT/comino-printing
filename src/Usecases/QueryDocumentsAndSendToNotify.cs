using System;
using System.Threading.Tasks;
using Boundary.UseCaseInterfaces;
using Usecases.Enums;
using Usecases.GatewayInterfaces;
using UseCases.GatewayInterfaces;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

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

            LambdaLogger.Log("Document ids retrieved: " + JsonConvert.SerializeObject(documents));

            documents.ForEach(async document => {
                LambdaLogger.Log($"Sending document. ID: {document.Id}");

                try{
                    LambdaLogger.Log("Fetching PDF");
                    var pdfBytesResponse = _s3Gateway.GetPdfDocumentAsByteArray(document.Id);
                    LambdaLogger.Log("Fetched from S3");

                    var sentStatus = _cominoGateway.GetDocumentSentStatus(document.CominoDocumentNumber);

                    LambdaLogger.Log($"Got send status from comino {JsonConvert.SerializeObject(sentStatus)}");
                    if (sentStatus.Printed)
                    {
                        LambdaLogger.Log($"Document already printed. ID: {document.Id}");
                        await _localDatabaseGateway.UpdateStatus(document.Id, LetterStatusEnum.PrintedManually);
                        await _logger.LogMessage(document.Id,
                            $"Not sent to GovNotify. Document already printed, printed at {sentStatus.PrintedAt}");
                        return;
                    }

                    var govNotifyResponse = _govNotifyGateway.SendPdfDocumentForPostage(pdfBytesResponse, document.Id);

                    if (govNotifyResponse.Success)
                    {
                        LambdaLogger.Log($"Document sent to notify. ID: {document.Id}");
                        _localDatabaseGateway.UpdateStatus(document.Id, LetterStatusEnum.SentToGovNotify).Wait();
                        _localDatabaseGateway.SaveSendNotificationId(document.Id, govNotifyResponse.NotificationId).Wait();
                        _logger.LogMessage(document.Id,
                            $"Sent to Gov Notify. Gov Notify Notification Id {govNotifyResponse.NotificationId}").Wait();
                        _cominoGateway.MarkDocumentAsSent(document.CominoDocumentNumber);
                        _logger.LogMessage(document.Id, "Removed from batch print queue and print date set in comino").Wait();
                    }
                    else
                    {
                        LambdaLogger.Log($"Error sending to notify. ID: {document.Id}");
                        _localDatabaseGateway.UpdateStatus(document.Id, LetterStatusEnum.FailedToSend).Wait();
                        _logger.LogMessage(document.Id, $"Error Sending to GovNotify: {govNotifyResponse.Error}").Wait();
                    }
                }catch(Exception e){
                    LambdaLogger.Log(JsonConvert.SerializeObject(e));
                    throw;
                }
            });
        }
    }
}
