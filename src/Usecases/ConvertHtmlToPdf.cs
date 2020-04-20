using AwsDotnetCsharp.UsecaseInterfaces;

namespace UseCases
{
    public class ConvertHtmlToPdf : IConvertHtmlToPdf
    {
        public string Execute(string htmlDocument)
        {
            return "[pdf document - filepath?]";
        }
    }
}
