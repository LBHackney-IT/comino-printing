using System.Threading.Tasks;
using Usecases.Domain;

namespace UseCases.GatewayInterfaces
{
    public interface ILocalDatabaseGateway
    {
        Task<string> SaveDocument(DocumentDetails newDocument);
    }
}