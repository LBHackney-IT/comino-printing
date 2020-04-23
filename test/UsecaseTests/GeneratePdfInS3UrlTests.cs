using Amazon.S3;
using Amazon.S3.Model;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Usecases.Domain;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class GeneratePdfInS3UrlTests
    {
        private GeneratePdfInS3Url _subject;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _subject = new GeneratePdfInS3Url();
            _fixture = new Fixture();
            ;
        }

        [Test]
        public void ExecuteReturnsAnS3UrlForTheGeneratedPdf()
        {
            var idFromRequest = _fixture.Create<DocumentDetails>().SavedAt;
                
            var response = _subject.Execute(idFromRequest);

            response.Should().Contain(idFromRequest);
        }
    }
}