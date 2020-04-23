using System.Collections.Generic;
using Usecases.Domain;

namespace Usecases.Interfaces
{
    public interface IGetDocumentsIds
    {
        List<DocumentDetails> Execute();
    }
}