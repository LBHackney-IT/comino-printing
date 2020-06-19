
using System.Collections.Generic;

namespace Usecases.Domain
{
    public class LetterTemplate
    {
        public string LetterHead { get; set; } = "";
        public string MainBody { get; set; }
        public string TemplateSpecificCss { get; set; }
        public string RightSideOfHeader { get; set; }
        public List<string> AddressLines { get; set; }
    }
}