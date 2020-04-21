using System.Threading.Tasks;
using Usecases.Domain;

namespace Usecases.UseCaseInterfaces
{
    public interface IGetDetailsOfDocumentForProcessing
    {
        Task<DocumentDetails> Execute(string timeStamp);
    }
}