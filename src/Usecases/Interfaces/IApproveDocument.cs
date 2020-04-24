using System.Threading.Tasks;

namespace Usecases.Interfaces
{
    public interface IApproveDocument
    {
        Task Execute(string id);
    }
}