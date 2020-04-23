using System.Threading.Tasks;
using comino_print_api.Models;

namespace Boundary.UseCaseInterfaces
{
    public interface IGetAllDocuments
    {
        Task<GetAllDocumentsResponse> Execute();
    }
}