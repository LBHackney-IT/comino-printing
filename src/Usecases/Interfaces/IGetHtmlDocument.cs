using System.Threading.Tasks;

namespace Usecases.Interfaces
{
    public interface IGetHtmlDocument
    {
        Task<string> Execute(string documentId);
    }
}