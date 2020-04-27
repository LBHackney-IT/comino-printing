using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using Usecases;
using Usecases.Domain;
using Usecases.Enums;
using Usecases.GatewayInterfaces;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class CheckSendStatusOfLettersTests
    {
        private Mock<ILocalDatabaseGateway> _localDbGateway;
        private Mock<IGovNotifyGateway> _govNotifyGateway;
        private Mock<IDbLogger> _logger;
        private CheckSendStatusOfLetters _subject;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _localDbGateway = new Mock<ILocalDatabaseGateway>();
            _govNotifyGateway = new Mock<IGovNotifyGateway>();
            _logger = new Mock<IDbLogger>();
            _subject = new CheckSendStatusOfLetters(_localDbGateway.Object, _govNotifyGateway.Object, _logger.Object);
        }

        [Test]
        public async Task ItGetsAllTheLetterWhichNeedCheckingFromTheLocalDb()
        {
            var letters = SetupLocalDatabaseToGetLettersToCheck();
            SetupGovNotifyToReturnStatus(letters);
            SetupLocalDatabaseToUpdateStatus(letters);

            await _subject.Execute();
            _localDbGateway.Verify(x => x.GetLettersWaitingForGovNotify());
        }

        [Test]
        public async Task ItSendsEachOfTheseLettersIdsToTheGovNotifyGateway()
        {
            var letters = SetupLocalDatabaseToGetLettersToCheck();
            SetupGovNotifyToReturnStatus(letters);
            SetupLocalDatabaseToUpdateStatus(letters);

            await _subject.Execute();
            _govNotifyGateway.VerifyAll();
        }

        [TestCase(LetterStatusEnum.GovNotifyPendingVirusCheck)]
        [TestCase(LetterStatusEnum.GovNotifyVirusScanFailed)]
        [TestCase( LetterStatusEnum.GovNotifyValidationFailed)]
        [TestCase(LetterStatusEnum.LetterSent)]
        public async Task ItSendsTheNewStatusesToTheLocalDatabaseToUpdate(LetterStatusEnum newStatus)
        {
            var letters = SetupLocalDatabaseToGetLettersToCheck();
            SetupGovNotifyToReturnStatus(letters, newStatus);
            SetupLocalDatabaseToUpdateStatus(letters, newStatus);
            await _subject.Execute();

            foreach (var letter in letters)
            {
                _localDbGateway.Verify(x => x.UpdateStatus(letter.Id, newStatus));
            }

        }

        [TestCase(LetterStatusEnum.GovNotifyPendingVirusCheck, "Gov Notify status: Pending Virus Check")]
        [TestCase(LetterStatusEnum.GovNotifyVirusScanFailed, "Gov Notify status: Virus Scan Failed")]
        [TestCase( LetterStatusEnum.GovNotifyValidationFailed, "Gov Notify status: Validation Failed")]
        [TestCase(LetterStatusEnum.LetterSent, "Gov Notify status: Letter Sent")]
        public async Task IfTheStatusHasChanged_LogsThisInTheDatabase(LetterStatusEnum status, string expectedMessage)
        {
            var letters = SetupLocalDatabaseToGetLettersToCheck();
            SetupGovNotifyToReturnStatus(letters, status);
            SetupLocalDatabaseToUpdateStatus(letters, status);

            await _subject.Execute();
            foreach (var letter in letters)
            {
                _logger.Verify(l => l.LogMessage(letter.Id, expectedMessage));
            }
        }

        private void SetupLocalDatabaseToUpdateStatus(List<DocumentDetails> letters,
            LetterStatusEnum status = LetterStatusEnum.GovNotifyPendingVirusCheck, bool statusUpdated = true)
        {
            foreach (var letter in letters)
            {
                _localDbGateway.Setup(x => x.UpdateStatus(letter.Id, status))
                    .ReturnsAsync(new UpdateStatusResponse{StatusUpdated = statusUpdated}).Verifiable();
            }
        }

        private void SetupGovNotifyToReturnStatus(IEnumerable<DocumentDetails> letters, LetterStatusEnum status = LetterStatusEnum.GovNotifyPendingVirusCheck)
        {
            foreach (var letter in letters)
            {
                _govNotifyGateway.Setup(x => x.GetStatusForLetter(letter.Id, letter.GovNotifyNotificationId))
                    .Returns(new GovNotifyResponse
                    {
                        Status = status,
                        SentAt = null
                    }).Verifiable();
            }
        }

        private List<DocumentDetails> SetupLocalDatabaseToGetLettersToCheck()
        {
            var letters = _fixture.CreateMany<DocumentDetails>().ToList();
            _localDbGateway.Setup(x => x.GetLettersWaitingForGovNotify()).ReturnsAsync(letters).Verifiable();
            return letters;
        }
    }
}