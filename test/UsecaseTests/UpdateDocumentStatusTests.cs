using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using comino_print_api.Responses;
using Moq;
using NUnit.Framework;
using UseCases;
using Usecases.Domain;
using Usecases.Enums;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class UpdateDocumentStatusTests
    {
        private Mock<ILocalDatabaseGateway> _dbGatewayMock;
        private UpdateDocumentStatus _subject;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _dbGatewayMock = new Mock<ILocalDatabaseGateway>();
            _subject = new UpdateDocumentStatus(_dbGatewayMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void ExecuteWillUpdateTheStatusOfTheDocument()
        {
            var putRequestId = _fixture.Create<DocumentDetails>().SavedAt;
            var requestedStatus = _fixture.Create<DocumentDetails>().Status;

            _subject.Execute(putRequestId, requestedStatus.ToString());
            
            _dbGatewayMock.Verify(x => x.UpdateStatus(putRequestId, requestedStatus));
        }
    }
}