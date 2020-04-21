using System.Threading.Tasks;

namespace UseCases.GatewayInterfaces
{
    public interface IW2DocumentsGateway
    {
        Task<string> GetHtmlDocument(string documentId);
    }
}
