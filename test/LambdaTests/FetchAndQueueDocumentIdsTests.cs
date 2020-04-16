using System.Data;
using Amazon.Lambda.Core;
using AutoFixture;
using AwsDotnetCsharp;
using Moq;
using NUnit.Framework;

namespace LambdaTests
{
    public class GetDocumentsTests
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void WhenInvokedItCallsGetDocumentsIds()
        {
            var lambda = new Handlers();
            var contextMock = new Mock<ILambdaContext>();
//            lambda.FetchAndQueueDocumentIds(contextMock.Object);
            //E2E Test Here - How to mock DB?
        }
    }
}