using System.Collections.Generic;
using Usecases.Domain;

namespace Usecases.UseCaseInterfaces
{
    public interface IGetDocumentsIds
    {
        List<DocumentDetails> Execute();
    }
}