using System.Threading.Tasks;

namespace Usecases.Interfaces
{
    public interface ICancelDocument
    {
        Task Execute(string id);
    }
}