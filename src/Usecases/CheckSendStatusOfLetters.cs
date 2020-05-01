using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Boundary.UseCaseInterfaces;
using Usecases.GatewayInterfaces;
using UseCases.GatewayInterfaces;
using Notify.Exceptions;

namespace Usecases
{
    public class CheckSendStatusOfLetters : ICheckSendStatusOfLetters
    {
        private readonly ILocalDatabaseGateway _localDatabaseGateway;
        private readonly IGovNotifyGateway _govNotifyGateway;
        private readonly IDbLogger _logger;

        public CheckSendStatusOfLetters(ILocalDatabaseGateway localDatabaseGateway, IGovNotifyGateway govNotifyGateway, IDbLogger logger)
        {
            _localDatabaseGateway = localDatabaseGateway;
            _govNotifyGateway = govNotifyGateway;
            _logger = logger;
        }

        public async Task Execute()
        {
            var lettersToCheck = await _localDatabaseGateway.GetLettersWaitingForGovNotify();
            lettersToCheck.ForEach(async letter =>
            {
                try{
                    LambdaLogger.Log($"Checking status for letter {letter.Id}, doc no {letter.CominoDocumentNumber} Notify ID: {letter.GovNotifyNotificationId}");
                    var govNotifyResponse = _govNotifyGateway.GetStatusForLetter(letter.Id, letter.GovNotifyNotificationId);
                    var updateStatusTask = _localDatabaseGateway.UpdateStatus(letter.Id, govNotifyResponse.Status);
                    updateStatusTask.Wait();
                    var updateResponse = updateStatusTask.Result;
                    LambdaLogger.Log($"Updated status in Local DB to {govNotifyResponse.Status}");
                    if (updateResponse.StatusUpdated)
                    {
                        _logger.LogMessage(letter.Id, $"Gov Notify status: {govNotifyResponse.Status.PrettierStatusName()}").Wait();
                    }
                }catch(NotifyClientException e){
                    LambdaLogger.Log($"Error checking status of document. ({e.Message})");
                }
            });
        }
    }
}