using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.SQSEvents;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using UseCases;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class ListenForSqsEventsTests
    {
        private ListenForSqsEvents _listenForSqsEvents;
        private Mock<SQSEvent> _sqsEventMock;
        private Mock<SQSEvent.SQSMessage> _sqsMessageMock;

        [SetUp]
        public void Setup()
        {
            _listenForSqsEvents = new ListenForSqsEvents();

            _sqsMessageMock = new Mock<SQSEvent.SQSMessage>();
            _sqsMessageMock.Object.Body = "123456";
            _sqsMessageMock.Object.EventSourceArn = "arn:aws:sqs:eu-west-2:123456789012:DefaultQueue";
            
            _sqsEventMock = new Mock<SQSEvent>();
            _sqsEventMock.Object.Records = new List<SQSEvent.SQSMessage>(){ _sqsMessageMock.Object };
        }

        [Test]
        public void ExecuteReceivesAndProcessesDocumentsFromSqs()
        {
            var expected = new List<string>(){ "123456" };
            var received = _listenForSqsEvents.Execute(_sqsEventMock.Object);

            received.Should().BeEquivalentTo(expected);
        }
    }
}
