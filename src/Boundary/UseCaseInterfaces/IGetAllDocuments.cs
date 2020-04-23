using System.Threading.Tasks;
using comino_print_api.Models;
using UseCases;

namespace Boundary.UseCaseInterfaces
{
    public interface IGetAllDocuments
    {
        Task<GetAllDocumentsResponse> Execute(EndIdParameter endIdParameter);
    }
}