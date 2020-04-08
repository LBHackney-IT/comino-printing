using System;
using System.Collections.Generic;

namespace RtfParseSpike.Templates
{
    public class IncomeVerificationTemplate
    {
        public string Title { get; set; }
        public string ClaimNumber { get; set; }
        public string Name { get; set; }
        public List<string> Address { get; set; }
        public string Postcode { get; set; }
        public List<List<string>> PeriodStartDateTable { get; set; }
        public List<List<string>> ReasonForAssessmentTable { get; set; }
        public List<List<string>> IncomeDetailsTable { get; set; }
        public string OverpaymentTableHeaders { get; set; }
        public List<List<string>> OverpaymentTable { get; set; }
        public List<List<string>> AdditionalCommentTable { get; set; }
     }
}