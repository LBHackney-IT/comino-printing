using System.Threading.Tasks;

namespace Usecases.GatewayInterfaces
{
    public interface IDbLogger
    {
        Task LogMessage(string documentSavedAt, string message);
    }
}