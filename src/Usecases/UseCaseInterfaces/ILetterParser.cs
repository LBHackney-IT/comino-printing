using RtfParseSpike.Templates;

namespace Usecases.UseCaseInterfaces
{
    public interface ILetterParser
    {
        LetterTemplate Execute(string html);
    }
}