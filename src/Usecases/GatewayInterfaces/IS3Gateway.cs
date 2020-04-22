using System.Threading.Tasks;
using Usecases.Domain;

namespace UseCases.GatewayInterfaces
{
    public interface IS3Gateway
    {
        Task<Response> SavePdfDocument(string documentId);
    }
}