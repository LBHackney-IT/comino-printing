using System.Threading.Tasks;
using Boundary.UseCaseInterfaces;
using Usecases.GatewayInterfaces;
using UseCases.GatewayInterfaces;

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
                var govNotifyResponse = _govNotifyGateway.GetStatusForLetter(letter.Id, letter.GovNotifyNotificationId);
                var updateResponse = await _localDatabaseGateway.UpdateStatus(letter.Id, govNotifyResponse.Status);
                if (updateResponse.StatusUpdated)
                {
                    await _logger.LogMessage(letter.Id, $"Gov Notify status: {govNotifyResponse.Status.PrettierStatusName()}");
                }
            });
        }
    }
}