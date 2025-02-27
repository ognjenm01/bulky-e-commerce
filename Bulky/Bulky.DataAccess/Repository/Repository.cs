using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Bulky.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public T? Get(Expression<Func<T, bool>> filter, string? includeProeprties = null, bool tracked = false)
        {
            IQueryable<T> query = dbSet;
            
            if (!tracked)
                query = dbSet.AsNoTracking();

            query = query.Where(filter);

            if (!string.IsNullOrEmpty(includeProeprties))
            {
                foreach (var prop in includeProeprties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(prop);
                }
            }

            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll(string? includeProeprties = null)
        {
            IQueryable<T> query = dbSet;

            if (!string.IsNullOrEmpty(includeProeprties))
            {
                foreach (var prop in includeProeprties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(prop);
                }
            }

            return query.ToList();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }
    }
}
