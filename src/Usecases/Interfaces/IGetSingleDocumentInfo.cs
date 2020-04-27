using System.Threading.Tasks;
using comino_print_api.Models;

namespace Usecases.Interfaces
{
    public interface IGetSingleDocumentInfo
    {
        Task<SingleDocumentResponse> Execute(string id);
    }
}