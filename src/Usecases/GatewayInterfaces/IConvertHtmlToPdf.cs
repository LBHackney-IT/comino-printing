using System.Threading.Tasks;

namespace Usecases.GatewayInterfaces
{
    public interface IConvertHtmlToPdf
    {
        Task Execute(string htmlDocument, string documentType, string documentId);
    }
}