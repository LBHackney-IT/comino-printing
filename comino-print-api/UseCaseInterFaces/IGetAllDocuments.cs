using System.Threading.Tasks;
using comino_print_api.Models;

namespace comino_print_api.UseCaseInterFaces
{
    public interface IGetAllDocuments
    {
        Task<GetAllDocumentsResponse> Execute();
    }
}