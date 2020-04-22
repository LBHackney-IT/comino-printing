using System.Threading.Tasks;
using Usecases.Domain;

namespace Usecases.GatewayInterfaces
{
    public interface IDbLogger
    {
        Task LogMessage(string documentSavedAt, string message);

        DocumentLog GetLogForDocument(string savedDocumentSavedAt);
    }
}