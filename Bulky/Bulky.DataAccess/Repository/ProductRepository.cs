using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            Product? pr = _db.Products.FirstOrDefault(p => p.Id == product.Id);

            if (pr != null)
            {
                pr.Title = product.Title;
                pr.Description = product.Description;
                pr.CategoryId = product.CategoryId;
                pr.Price = product.Price;
                pr.ListPrice = product.ListPrice;
                pr.Price50 = product.Price50;
                pr.Price100 = product.Price100;
                pr.ISBN = product.ISBN;
                pr.Author = product.Author;
                if (product.ImageUrl != null)
                {
                    pr.ImageUrl = product.ImageUrl;
                }
            }

            _db.Products.Update(pr);
        }
    }
}
