using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;

namespace Usecases.Interfaces
{
    public interface IUpdateDocumentState
    {
        Task Execute(string id, string status);
    }
}