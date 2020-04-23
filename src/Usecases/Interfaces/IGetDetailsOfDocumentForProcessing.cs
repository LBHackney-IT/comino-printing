using System.Threading.Tasks;
using Usecases.Domain;

namespace Usecases.Interfaces
{
    public interface IGetDetailsOfDocumentForProcessing
    {
        Task<DocumentDetails> Execute(string timeStamp);
    }
}