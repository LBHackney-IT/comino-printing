using comino_print_api.Models;
using Microsoft.EntityFrameworkCore;

namespace comino_print_api.Contexts
{
    public class AppDbContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<Document>().ToTable("Documents");
            builder.Entity<Document>().Property(p => p.DocumentId).IsRequired();
            builder.Entity<Document>().Property(p => p.DocumentCreator).IsRequired();
            builder.Entity<Document>().Property(p => p.CreatedAt).IsRequired();

            builder.Entity<Document>().HasData
            (
                new Document { DocumentId = "100200", DocumentCreator = "Mr Foo Bar" , CreatedAt = "Some date"},
                new Document { DocumentId = "2003333", DocumentCreator = "Mr Bar Baz", CreatedAt = "Some date" }
            );
        }
    }
}