using System.Collections.Generic;
using System.Threading.Tasks;
using comino_print_api.Models;

namespace comino_print_api.Repositories
{
    public interface IDocumentRepository
    {
        Task<IEnumerable<Document>> ListAsync();
    }
}