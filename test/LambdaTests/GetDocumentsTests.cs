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
        private Mock<IGetDocumentsIds> _usecaseMock;
        
        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _usecaseMock = new Mock<IGetDocumentsIds>();
        }

        [Test]
        public void WhenInvokedItCallsGetDocumentsIds()
        {
            var lambda = new GetDocuments(_usecaseMock.Object);
            var contextMock = new Mock<ILambdaContext>();
            lambda.FetchDocumentIds(contextMock.Object);
            
            _usecaseMock.Verify(x => x.Execute(), Times.Once);
        }
    }
}