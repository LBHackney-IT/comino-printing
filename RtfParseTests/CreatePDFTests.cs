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
        private DirectoryInfo htmlLetters;
        private CreatePDF _subject;
        private DirectoryInfo _pdfResults;

        [SetUp]
        public void SetUp()
        {
            htmlLetters = new DirectoryInfo("./../../../TestFixtures/ChangesInCircsICL/ExampleLettersHTML");
            _pdfResults = new DirectoryInfo("./../../../TestFixtures/ChangesInCircsICL/PdfResults");

            _subject = new CreatePDF();
        }

        [Test]
        public void ConvertsToPDF()
        {
            htmlLetters.GetFiles().ToList().ForEach(fileInfo =>
            {
                var fileName = fileInfo.Name.ToString().Split('.').First();

                var templateConverts = new ChangesInCircsICL();
                var htmlInput = templateConverts.Execute(fileInfo);
                var pdfBytes = _subject.Execute(htmlInput);

                File.WriteAllBytesAsync(_pdfResults.ToString() + $"/{fileName}.pdf", pdfBytes);
                File.Exists(_pdfResults.ToString() + $"/{fileName}.pdf").Should().BeTrue();
            });
        }
    }
}