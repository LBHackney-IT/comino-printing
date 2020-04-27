using System.Threading.Tasks;
using Usecases.Domain;

namespace Usecases.GatewayInterfaces
{
    public interface IDbLogger
    {
        Task LogMessage(string id, string message);

        DocumentLog GetLogForDocument(string id);
    }
}