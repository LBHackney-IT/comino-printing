
using Usecases.Domain;

namespace Usecases.Interfaces
{
    public interface ILetterParser
    {
        LetterTemplate Execute(string html);
    }
}