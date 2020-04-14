using System;
using System.Collections.Generic;

namespace RtfParseSpike.Templates
{
    public class BlankBenefitsTemplate
    {
        public List<List<string>> AddressFields { get; set; }
        public string Greeting { get; set; }
        public string LetterBody { get; set; }
        public string LetterClosing { get; set; }
    }
}
