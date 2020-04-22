using System.Collections.Generic;
using System.Threading.Tasks;
using comino_print_api.Contexts;
using comino_print_api.Models;
using Microsoft.EntityFrameworkCore;

namespace comino_print_api.Repositories
{
    public class DocumentRepository : BaseRepository, IDocumentRepository
    {
        public DocumentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Document>> ListAsync()
        {
            return await _context.Documents.ToListAsync();
        }
    }
}