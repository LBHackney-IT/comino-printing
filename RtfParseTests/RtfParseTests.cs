using FluentAssertions;
using NUnit.Framework;
using RtfParseSpike;

namespace RtfParseTests
{
    public class RtfParseTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ExecuteReturnsNull()
        {
            var spike = new RtfParse();
            spike.Execute().Should().BeNull();
        }
    }
}