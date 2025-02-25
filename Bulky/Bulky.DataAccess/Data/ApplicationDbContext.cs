using Bulky.Models;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
             
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "SciFi", DisplayOrder = 2 },
                new Category { Id = 3, Name = "History", DisplayOrder = 3 });

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, ISBN = "9781784876555", Author = "George Orwell", Title = "1984", Description = "A dystopian masterpiece, this is the powerful and prophetic novel that defined the twentieth century.", ListPrice = 100, Price = 100, Price50 = 75, Price100 = 50 },
                new Product { Id = 2, ISBN = " 9780060120351", Author = "Aldous Huxley", Title = "Brave New World", Description = "The astonishing novel Brave New World, originally published in 1932, presents Aldous Huxley's vision of the future", ListPrice = 120, Price = 120, Price50 = 100, Price100 = 80 },
                new Product { Id = 3, ISBN = "9781501156687", Author = "Stephen King", Title = "It", Description = "Can an entire city be haunted? The Losers’ Club of 1958 seems to think so.", ListPrice = 130, Price = 130, Price50 = 105, Price100 = 95 });
        }
    }
}
