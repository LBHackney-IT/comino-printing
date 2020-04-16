using Amazon.Lambda.Core;
using AutoFixture;
using AwsDotnetCsharp;
using AwsDotnetCsharp.UsecaseInterfaces;
using Moq;
using NUnit.Framework;

namespace LambdaTests
{
    public class GetDocumentsTests
    {
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void WhenInvokedItCallsGetDocumentsIds()
        {
            var lambda = new GetDocuments();
            var contextMock = new Mock<ILambdaContext>();
            lambda.FetchDocumentIds(contextMock.Object);
            //E2E Test Here?
        }
    }
}