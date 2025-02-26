using Bulky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.Data
{
    public class ApplicationDbContext :IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
             
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder); //IdentityDbContext

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "SciFi", DisplayOrder = 2 },
                new Category { Id = 3, Name = "History", DisplayOrder = 3 },
                new Category { Id = 4, Name = "Distopia", DisplayOrder = 4 },
                new Category { Id = 5, Name = "Horror", DisplayOrder = 5 });

            modelBuilder.Entity<Product>().HasData(
                new Product { 
                    Id = 1, 
                    ISBN = "9781784876555", 
                    Author = "George Orwell", 
                    Title = "1984", 
                    Description = "A dystopian masterpiece, this is the powerful and prophetic novel that defined the twentieth century.", 
                    ListPrice = 100, 
                    Price = 100,
                    Price50 = 75, 
                    Price100 = 50, 
                    CategoryId = 4, 
                    ImageUrl = "https://ih1.redbubble.net/image.3627578336.5377/flat,750x,075,f-pad,750x1000,f8f8f8.jpg" 
                },
                
                new Product { 
                    Id = 2, 
                    ISBN = "9780060120351", 
                    Author = "Aldous Huxley", 
                    Title = "Brave New World", 
                    Description = "The astonishing novel Brave New World, originally published in 1932, presents Aldous Huxley's vision of the future", 
                    ListPrice = 120, 
                    Price = 120, 
                    Price50 = 100, 
                    Price100 = 80, 
                    CategoryId = 4, 
                    ImageUrl = "https://www.redmolotov.com/image/cache/catalog/books/brave-new-world-book_design-1000x1000.jpg" 
                },
                
                new Product { 
                    Id = 3, 
                    ISBN = "9781501156687", 
                    Author = "Stephen King", 
                    Title = "It", 
                    Description = "Can an entire city be haunted? The Losers’ Club of 1958 seems to think so.", 
                    ListPrice = 130, 
                    Price = 130, 
                    Price50 = 105, 
                    Price100 = 95 , 
                    CategoryId = 5, 
                    ImageUrl= "https://m.media-amazon.com/images/I/71KWpItR2kL.jpg" });

            modelBuilder.Entity<Company>().HasData(
            new Company { Id = 1, 
                Name = "Name1", 
                StreetAddress = "Street1", 
                City="City1", 
                State="State1", 
                PostalCode="PostalCode1", 
                PhoneNumber="Number1" },
            new Company
            {
                Id = 2,
                Name = "Name2",
                StreetAddress = "Street2",
                City = "City2",
                State = "State2",
                PostalCode = "PostalCode2",
                PhoneNumber = "Number2"
            },
            new Company
            {
                Id = 3,
                Name = "Name3",
                StreetAddress = "Street3",
                City = "City3",
                State = "State3",
                PostalCode = "PostalCode3",
                PhoneNumber = "Number3"
            });
        }
    }
}
