using AutoFixture;
using comino_print_api.Responses;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UseCases;
using Usecases.Domain;
using Usecases.Enums;
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

        [SetUp]
        public void Setup()
        {
            _dbGatewayMock = new Mock<ILocalDatabaseGateway>();
            _s3GatewayMock = new Mock<IS3Gateway>();
            _govNotifyGatewayMock = new Mock<IGovNotifyGateway>();

            _subject = new QueryDocumentsAndSendToNotify(
                _dbGatewayMock.Object,
                _s3GatewayMock.Object,
                _govNotifyGatewayMock.Object
            );

            _fixture = new Fixture();
        }

        [Test]
        [Ignore("todo")]
        public void ExecuteSendsCorrectDocumentsToNotify()
        {
            // this usecase:
            // - gets the readytosend docs from localdb
            // - for each readytosend doc, gets the pdf byte array from s3
            //   and dispatches to notify

            // to test:
            // - are s3 gateway and govnotify gateway called with expected
            //   response from localdb gateway?

            var savedRecords = _fixture.CreateMany<DocumentDetails>().ToList();
            savedRecords.ForEach(record => record.Status = LetterStatusEnum.ReadyForGovNotify);

            var localDbResponse = convertRecordsToResponse(savedRecords);

            _dbGatewayMock.Setup(x => x.GetDocumentsThatAreReadyForGovNotify()).ReturnsAsync(savedRecords);

            foreach (var document in localDbResponse)
            {
                _s3GatewayMock
                    .Verify(x => x.GetPdfDocumentAsByteArray(document.DocNo), Times.Once);

                _govNotifyGatewayMock
                    .Verify(x => x.SendPdfDocumentForPostage(new byte[] {}, "uniqueRef"), Times.Once);
            }
        }

        private IEnumerable<DocumentResponse> convertRecordsToResponse(List<DocumentDetails> savedRecords)
        {
            return savedRecords.Select(record => new DocumentResponse
            {
                Id = record.Id,
                DocNo = record.CominoDocumentNumber,
                Sender = record.DocumentCreator,
                Created = record.Id,
                Status = record.Status.ToString(),
                LetterType = record.LetterType,
                DocumentType = record.DocumentType,
                Logs = record.Log.Select(x => new Dictionary<string, string>
                {
                    {"date", x.Key},
                    {"message", x.Value}
                }).ToList()
            });
        }
    }
}