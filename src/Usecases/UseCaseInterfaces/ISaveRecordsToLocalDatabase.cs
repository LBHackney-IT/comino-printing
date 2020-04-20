using System.Collections.Generic;
using System.Threading.Tasks;
using Usecases.Domain;

namespace Usecases.UseCaseInterfaces
{
    public interface ISaveRecordsToLocalDatabase
    {
        Task<List<DocumentDetails>> Execute(List<DocumentDetails> documentsToSave);
    }
}