using System.Collections.Generic;
using System.Threading.Tasks;
using comino_print_api.Models;

// using JsonApiDotNetCore.Models;

namespace comino_print_api.Services
{
    public interface IDocumentService
    {
        Task<IEnumerable<Document>> ListAsync();
    }
}