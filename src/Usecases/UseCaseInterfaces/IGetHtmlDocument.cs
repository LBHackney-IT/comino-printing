using System.Threading.Tasks;

namespace Usecases.UseCaseInterfaces
{
    public interface IGetHtmlDocument
    {
        Task<string> Execute(string documentId);
    }
}