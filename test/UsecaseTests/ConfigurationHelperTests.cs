using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Usecases;

namespace UnitTests
{
    public class ConfigurationHelperTests
    {
        [Test]
        public void SetsDocumentConfigurationEnvironmentVariableWithCorrectValues()
        {
            var expected = new DocumentConfig
            {
                Categories = new List<string> { "Benefits/Out-Going" },
                Descriptions = new List<string> { "Benefits Blank Letter", "Change in Circs ICL" },
                AutomaticApprovals = new List<string> { "Benefits Blank Letter" }
            };

            ConfigurationHelper.SetDocumentConfigEnvironmentVariable();

            var received = Environment.GetEnvironmentVariable("DOCUMENT_CONFIG");

            Assert.AreEqual(JsonConvert.SerializeObject(expected), received);
        }
    }
}
