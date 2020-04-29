using System.Threading.Tasks;
using Usecases.Domain;

namespace UseCases.GatewayInterfaces
{
    public interface IS3Gateway
    {
        Task<Response> SavePdfDocument(string documentId);

        //TODO Remove later - for troubleshooting PDF Generation
        Task<Response> SaveHtmlDocument(string documentId);
        string GeneratePdfUrl(string docId);
        Task<byte[]> GetPdfDocumentAsByteArray(string documentId);
    }
}
