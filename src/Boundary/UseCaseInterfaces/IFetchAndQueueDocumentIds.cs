using System.Threading.Tasks;

namespace Boundary.UseCaseInterfaces
{
    public interface IFetchAndQueueDocumentIds
    {
        Task Execute();
    }
}