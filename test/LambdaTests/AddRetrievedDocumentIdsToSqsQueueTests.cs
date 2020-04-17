using Amazon.Lambda.Core;
using AwsDotnetCsharp;
using Moq;
using NUnit.Framework;

namespace LambdaTests
{
    public class AddRetrievedDocumentIdsToSqsQueueTests
    {
        [SetUp]
        public void Setup()
        {
        }
        
        [Test]
        public void WhenInvokedItCallsPushIdsToSqs()
        {
            var lambda = new Handlers();
            var contextMock = new Mock<ILambdaContext>();
        }
    }
}