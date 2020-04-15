using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RtfParseSpike;
using RtfParseSpike.Parsers;

namespace RtfParseTests
{
    public class CreatePDFTests
    {
        private CreatePDF _subject;

        [SetUp]
        public void SetUp()
        {
            _subject = new CreatePDF();
        }

        [Test]
        public void ConvertsBenefitsBlankLetterToPDF()
        {
            var htmlLetters = new DirectoryInfo("./../../../TestFixtures/BenefitsBlankLetter/ExampleLettersHTML");
            var pdfResults = new DirectoryInfo("./../../../TestFixtures/BenefitsBlankLetter/PdfResults");
            htmlLetters.GetFiles().ToList().ForEach(fileInfo =>
            {
                var fileName = fileInfo.Name.ToString().Split('.').First();
                if(fileName == "") { return; }

                var templateConverts = new BlankBenefitsRtfParser();
                var htmlInput = templateConverts.Execute(fileInfo);
                var pdfBytes = _subject.Execute(htmlInput);

                File.WriteAllBytesAsync(pdfResults.ToString() + $"/{fileName}.pdf", pdfBytes);
                File.Exists(pdfResults.ToString() + $"/{fileName}.pdf").Should().BeTrue();
            });
        }

        [Test]
        public void ConvertsChangeInCircLettersToPDF()
        {
            var htmlLetters = new DirectoryInfo("./../../../TestFixtures/ChangesInCircsICL/ExampleLettersHTML");
            var pdfResults = new DirectoryInfo("./../../../TestFixtures/ChangesInCircsICL/PdfResults");

            htmlLetters.GetFiles().ToList().ForEach(fileInfo =>
            {
                var fileName = fileInfo.Name.ToString().Split('.').First();
                if(fileName == "") { return; }

                var templateConverts = new ChangesInCircsICL();
                var htmlInput = templateConverts.Execute(fileInfo);
                var pdfBytes = _subject.Execute(htmlInput);

                File.WriteAllBytesAsync(pdfResults.ToString() + $"/{fileName}.pdf", pdfBytes);
                File.Exists(pdfResults.ToString() + $"/{fileName}.pdf").Should().BeTrue();
            });
        }
    }
}