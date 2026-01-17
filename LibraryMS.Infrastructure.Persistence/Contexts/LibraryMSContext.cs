using LibraryMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LibraryMS.Infrastructure.Persistence.Contexts
{
    public class LibraryMSContext : DbContext
    {
        public LibraryMSContext(DbContextOptions<LibraryMSContext> opt) : base(opt) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<AccountRequest> AccountRequests { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }
    }
}
