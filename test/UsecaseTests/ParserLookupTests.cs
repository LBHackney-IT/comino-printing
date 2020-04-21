using FluentAssertions;
using NUnit.Framework;
using UseCases;
using Usecases.UntestedParsers;

namespace UnitTests
{
    public class ParserLookupTests
    {
        private ParserLookup _subject;

        [SetUp]
        public void SetUp()
        {
            _subject = new ParserLookup();
        }

        [Test]
        public void ChangeInCircsReturnsCorrectParser()
        {
            _subject.ForType("Change in Circs ICL").Should().BeOfType<ChangesInCircsICL>();
        }

        [Test]
        public void BenefitsBlankLetterReturnsCorrectParser()
        {
            _subject.ForType("Benefits Blank Letter").Should().BeOfType<BlankBenefitsRtfParser>();
        }
    }
}