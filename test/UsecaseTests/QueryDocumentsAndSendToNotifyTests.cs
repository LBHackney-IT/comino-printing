using AutoFixture;
using comino_print_api.Responses;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
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

        [SetUp]
        public void Setup()
        {
            _dbGatewayMock = new Mock<ILocalDatabaseGateway>();
            _s3GatewayMock = new Mock<IS3Gateway>();
            _govNotifyGatewayMock = new Mock<IGovNotifyGateway>();
            _logger = new Mock<IDbLogger>();

            _subject = new QueryDocumentsAndSendToNotify(
                _dbGatewayMock.Object,
                _s3GatewayMock.Object,
                _govNotifyGatewayMock.Object,
                _logger.Object
            );

            _fixture = new Fixture();
        }

        [Test]
        public async Task ExecuteSendsCorrectDocumentsToNotify()
        {
            // this usecase:
            // - gets the readytosend docs from localdb
            // - for each readytosend doc, gets the pdf byte array from s3
            //   and dispatches to notify

            // to test:
            // - are s3 gateway and govnotify gateway called with expected
            //   response from localdb gateway?

            var savedRecords = SetupLocalDbGatewayToReturnRandomDocuments();

            foreach (var document in savedRecords)
            {
                var pdf = _fixture.Create<byte[]>();
                _s3GatewayMock
                    .Setup(x => x.GetPdfDocumentAsByteArray(document.Id, document.CominoDocumentNumber))
                    .ReturnsAsync(pdf)
                    .Verifiable();
                _govNotifyGatewayMock
                    .Setup(x => x.SendPdfDocumentForPostage(pdf, document.CominoDocumentNumber))
                    .Returns(new GovNotifySendResponse())
                    .Verifiable();
            }

            await _subject.Execute();
            _s3GatewayMock.Verify();
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
                _logger.Verify(x => x.LogMessage(document.Id, $"Sent to Gov Notify. Gov Notify Notification Id {document.GovNotifyNotificationId}"));
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
                _s3GatewayMock
                    .Verify(x => x.GetPdfDocumentAsByteArray(document.Id, document.CominoDocumentNumber), Times.Once);
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
                _dbGatewayMock.Verify(x => x.UpdateStatus(document.Id, LetterStatusEnum.GovNotifySendError), Times.Once);
                _logger.Verify(x => x.LogMessage(document.Id, $"Error Sending to GovNotify: {errorMessageFromGovNotify}"));
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
