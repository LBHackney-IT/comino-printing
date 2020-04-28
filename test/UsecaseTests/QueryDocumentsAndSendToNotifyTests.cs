using AutoFixture;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UseCases;
using Usecases.Domain;
using Usecases.Enums;
using Usecases.GatewayInterfaces;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class QueryDocumentsAndSendToNotifyTests
    {
        private Mock<ILocalDatabaseGateway> _dbGatewayMock;
        private Mock<IS3Gateway> _s3GatewayMock;
        private Mock<IGovNotifyGateway> _govNotifyGatewayMock;
        private QueryDocumentsAndSendToNotify _subject;
        private Fixture _fixture;
        private Mock<IDbLogger> _logger;
        private Mock<ICominoGateway> _cominoGateway;

        [SetUp]
        public void Setup()
        {
            _dbGatewayMock = new Mock<ILocalDatabaseGateway>();
            _s3GatewayMock = new Mock<IS3Gateway>();
            _govNotifyGatewayMock = new Mock<IGovNotifyGateway>();
            _cominoGateway = new Mock<ICominoGateway>();
            _logger = new Mock<IDbLogger>();

            _subject = new QueryDocumentsAndSendToNotify(
                _dbGatewayMock.Object,
                _s3GatewayMock.Object,
                _govNotifyGatewayMock.Object,
                _cominoGateway.Object,
                _logger.Object
            );

            _fixture = new Fixture();
        }

        [Test]
        public async Task Execute_IfDocumentsHaveNOtAlreadyBeenSent_SendsCorrectDocumentsToNotify()
        {
            var savedRecords = SetupLocalDbGatewayToReturnRandomDocuments();

            foreach (var document in savedRecords)
            {
                var pdf = _fixture.Create<byte[]>();
                _s3GatewayMock
                    .Setup(x => x.GetPdfDocumentAsByteArray(document.Id, document.CominoDocumentNumber))
                    .ReturnsAsync(pdf)
                    .Verifiable();
                _cominoGateway.Setup(x => x.GetDocumentSentStatus(document.Id))
                    .Returns(new CominoSentStatusCheck {Printed = false});
                _govNotifyGatewayMock
                    .Setup(x => x.SendPdfDocumentForPostage(pdf, document.CominoDocumentNumber))
                    .Returns(new GovNotifySendResponse())
                    .Verifiable();
            }

            await _subject.Execute();
            _s3GatewayMock.Verify();
        }

        [Test]
        public async Task IfCominoReturnsThatDocumentHasBeenSent_DoesNotCallGovNotify()
        {
            var savedRecords = SetupLocalDbGatewayToReturnRandomDocuments();

            foreach (var document in savedRecords)
            {
                var pdf = _fixture.Create<byte[]>();
                _s3GatewayMock
                    .Setup(x => x.GetPdfDocumentAsByteArray(document.Id, document.CominoDocumentNumber))
                    .ReturnsAsync(pdf);

                _cominoGateway.Setup(x => x.GetDocumentSentStatus(document.Id)).Returns(new CominoSentStatusCheck{ Printed = true });
            }

            await _subject.Execute();
            _govNotifyGatewayMock.Verify(x => x.SendPdfDocumentForPostage(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task IfCominoReturnsThatDocumentHasBeenSent_UpdatesTheLogAndStatus()
        {
            var savedRecords = SetupLocalDbGatewayToReturnRandomDocuments();
            var printedAtDate = _fixture.Create<string>();

            foreach (var document in savedRecords)
            {
                var pdf = _fixture.Create<byte[]>();
                _s3GatewayMock
                    .Setup(x => x.GetPdfDocumentAsByteArray(document.Id, document.CominoDocumentNumber))
                    .ReturnsAsync(pdf);

                _cominoGateway.Setup(x => x.GetDocumentSentStatus(document.Id))
                    .Returns(new CominoSentStatusCheck{ Printed = true, PrintedAt = printedAtDate});
            }

            await _subject.Execute();
            foreach (var document in savedRecords)
            {
                _logger.Verify(x => x.LogMessage(document.Id, $"Not sent to GovNotify. Document already printed, printed at {printedAtDate}"), Times.Once);
                _dbGatewayMock.Verify(x => x.UpdateStatus(document.Id, LetterStatusEnum.PrintedManually), Times.Once);
            }
        }

        [Test]
        public async Task IfGovNotifyReturnsSuccessfully_ChangesDocumentStatusToSentToGovNotify_AndWritesToLog()
        {
            var savedRecords = SetupLocalDbGatewayToReturnRandomDocuments();
            SetUpGovNotifyGatewayToReturnSuccessfully(savedRecords);

            await _subject.Execute();
            foreach (var document in savedRecords)
            {
                _dbGatewayMock.Verify(x => x.UpdateStatus(document.Id, LetterStatusEnum.SentToGovNotify), Times.Once);
                _logger.Verify(x => x.LogMessage(document.Id, $"Sent to Gov Notify. Gov Notify Notification Id {document.GovNotifyNotificationId}."));
            }
        }

        [Test]
        public async Task IfGovNotifyReturnsSuccessfully_SavesGovNotifyId()
        {
            var savedRecords = SetupLocalDbGatewayToReturnRandomDocuments();
            SetUpGovNotifyGatewayToReturnSuccessfully(savedRecords);

            await _subject.Execute();
            foreach (var document in savedRecords)
            {
                _dbGatewayMock.Verify(x => x.SaveSendNotificationId(document.Id, document.GovNotifyNotificationId), Times.Once);
            }
        }

        [Test]
        public async Task IfGovNotifyReturnsSuccessfully_UpdateCominoGateway()
        {
            var savedRecords = SetupLocalDbGatewayToReturnRandomDocuments();
            SetUpGovNotifyGatewayToReturnSuccessfully(savedRecords);

            await _subject.Execute();
            foreach (var document in savedRecords)
            {
                _cominoGateway.Verify(x => x.MarkDocumentAsSent(document.Id), Times.Once);
            }
        }

        [Test]
        public async Task IfGovNotifyReturnsAnError_ChangesDocumentStatusToError_AndWritesToLog()
        {
            var savedRecords = SetupLocalDbGatewayToReturnRandomDocuments();
            var errorMessageFromGovNotify = _fixture.Create<string>();

            SetUpGovNotifyToReturnError(savedRecords, errorMessageFromGovNotify);

            await _subject.Execute();
            foreach (var document in savedRecords)
            {
                _dbGatewayMock.Verify(x => x.UpdateStatus(document.Id, LetterStatusEnum.FailedToSend), Times.Once);
                _logger.Verify(x => x.LogMessage(document.Id, $"Error Sending to GovNotify: {errorMessageFromGovNotify}"));
                _cominoGateway.Verify(x => x.MarkDocumentAsSent(document.Id), Times.Never);
            }
        }

        private void SetUpGovNotifyToReturnError(List<DocumentDetails> savedRecords, string errorMessageFromGovNotify)
        {
            foreach (var document in savedRecords)
            {
                var notificationId = _fixture.Create<string>();
                var pdf = _fixture.Create<byte[]>();

                _s3GatewayMock
                    .Setup(x => x.GetPdfDocumentAsByteArray(document.Id, document.CominoDocumentNumber))
                    .ReturnsAsync(pdf);
                _cominoGateway.Setup(x => x.GetDocumentSentStatus(document.Id))
                    .Returns(new CominoSentStatusCheck {Printed = false});
                _govNotifyGatewayMock
                    .Setup(x => x.SendPdfDocumentForPostage(pdf, document.CominoDocumentNumber))
                    .Returns(new GovNotifySendResponse
                    {
                        Success = false,
                        Error = errorMessageFromGovNotify
                    });
                document.GovNotifyNotificationId = notificationId;
            }
        }

        private void SetUpGovNotifyGatewayToReturnSuccessfully(List<DocumentDetails> savedRecords)
        {
            foreach (var document in savedRecords)
            {
                var notificationId = _fixture.Create<string>();
                var pdf = _fixture.Create<byte[]>();

                _s3GatewayMock
                    .Setup(x => x.GetPdfDocumentAsByteArray(document.Id, document.CominoDocumentNumber))
                    .ReturnsAsync(pdf);
                _cominoGateway.Setup(x => x.GetDocumentSentStatus(document.Id))
                    .Returns(new CominoSentStatusCheck {Printed = false});
                _govNotifyGatewayMock
                    .Setup(x => x.SendPdfDocumentForPostage(pdf, document.CominoDocumentNumber))
                    .Returns(new GovNotifySendResponse
                    {
                        Success = true,
                        NotificationId = notificationId
                    });
                document.GovNotifyNotificationId = notificationId;
            }
        }

        private List<DocumentDetails> SetupLocalDbGatewayToReturnRandomDocuments()
        {
            var savedRecords = _fixture.Build<DocumentDetails>()
                .With(d => d.Status, LetterStatusEnum.ReadyForGovNotify)
                .CreateMany().ToList();
            _dbGatewayMock.Setup(x => x.GetDocumentsThatAreReadyForGovNotify()).ReturnsAsync(savedRecords);
            return savedRecords;
        }
    }
}
