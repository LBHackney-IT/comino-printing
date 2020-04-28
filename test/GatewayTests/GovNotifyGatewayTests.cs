using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Gateways;
using Moq;
using Notify.Exceptions;
using Notify.Interfaces;
using Notify.Models;
using Notify.Models.Responses;
using NUnit.Framework;
using Usecases.Domain;
using Usecases.Enums;

namespace GatewayTests
{
    public class GovNotifyGatewayTests
    {
        private Mock<INotificationClient> _govNotifyMockClient;
        private GovNotifyGateway _subject;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _govNotifyMockClient = new Mock<INotificationClient>();
            _subject = new GovNotifyGateway(_govNotifyMockClient.Object);
        }

        [Test]
        public void SendPdfDocumentForPostage_CallsGovNotifyWithThePdf()
        {
            var pdfBytes = _fixture.Create<byte[]>();
            var reference = _fixture.Create<string>();
            _govNotifyMockClient.Setup(x => x.SendPrecompiledLetter(reference, pdfBytes, null))
                .Returns(new LetterNotificationResponse());
            _subject.SendPdfDocumentForPostage(pdfBytes, reference);
        }

        [Test]
        public void SendPdfDocumentForPostage_IfSuccessful_ReturnsGovNotifyNotificationId()
        {
            var pdfBytes = _fixture.Create<byte[]>();
            var reference = _fixture.Create<string>();
            var returnNotificationId = _fixture.Create<string>();
            _govNotifyMockClient.Setup(x => x.SendPrecompiledLetter(reference, pdfBytes, null))
                .Returns(new LetterNotificationResponse{id = returnNotificationId});

            var response = _subject.SendPdfDocumentForPostage(pdfBytes, reference);
            response.NotificationId.Should().BeEquivalentTo(returnNotificationId);
            response.Success.Should().BeTrue();
        }

        [Test]
        public void SendPdfDocumentForPostage_IfError_ReturnsErrors()
        {
            var pdfBytes = _fixture.Create<byte[]>();
            var reference = _fixture.Create<string>();
            var errorMessage = "my error message";

            _govNotifyMockClient.Setup(x => x.SendPrecompiledLetter(reference, pdfBytes, null))
                .Throws(new NotifyClientException(errorMessage));

            var response = _subject.SendPdfDocumentForPostage(pdfBytes, reference);
            response.Success.Should().BeFalse();
            response.Error.Should().Be(errorMessage);
        }

        [TestCase("pending-virus-check", LetterStatusEnum.GovNotifyPendingVirusCheck)]
        [TestCase("virus-scan-failed", LetterStatusEnum.GovNotifyVirusScanFailed)]
        [TestCase("validation-failed", LetterStatusEnum.GovNotifyValidationFailed)]

        public void GetStatusForLetter_CallsGovNotifyWithTheCorrectIdAndReturnsStatusInformation(string govNotifyStatus, LetterStatusEnum expectedStatus)
        {
            var notificationId = _fixture.Create<string>();

            var returnedNotification = _fixture.Create<Notification>();
            returnedNotification.status = govNotifyStatus;
            returnedNotification.completedAt = null;

            _govNotifyMockClient.Setup(x => x.GetNotificationById(notificationId)).Returns(returnedNotification);

            var response = _subject.GetStatusForLetter("doc ID", notificationId);
            response.Should().BeEquivalentTo(new GovNotifyStatusResponse
            {
                Status = expectedStatus,
                SentAt = null
            });
        }

        [Test]
        public void GetStatusForLetter_WhenLetterIsCompleted_AndNotFailed_ReturnsSent()
        {
            var notificationId = _fixture.Create<string>();

            var returnedNotification = _fixture.Create<Notification>();
            returnedNotification.status = null;
            returnedNotification.completedAt = DateTime.Now.ToString("O");

            _govNotifyMockClient.Setup(x => x.GetNotificationById(notificationId)).Returns(returnedNotification);

            var response = _subject.GetStatusForLetter("doc ID", notificationId);
            response.Should().BeEquivalentTo(new GovNotifyStatusResponse
            {
                Status = LetterStatusEnum.LetterSent,
                SentAt = returnedNotification.completedAt
            });
        }
    }
}