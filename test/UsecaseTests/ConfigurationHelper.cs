using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Usecases;

namespace UnitTests
{
    public class ConfigurationHelper
    {

        public static void SetDocumentConfigEnvironmentVariable()
        {
            var configObj = new DocumentConfig
            {
                Categories = new List<string> { "Benefits/Out-Going" },
                Descriptions = new List<string> { "Benefits Blank Letter", "Change in Circs ICL" },
                AutomaticApprovals = new List<string> { "Benefits Blank Letter" }
            };

            Environment.SetEnvironmentVariable("DOCUMENT_CONFIG", JsonConvert.SerializeObject(configObj));
        }
    }
}
