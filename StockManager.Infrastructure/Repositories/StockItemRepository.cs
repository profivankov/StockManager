using StockManager.Core.Entities;
using StockManager.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StockManager.Infrastructure.Repositories
{
    public class StockItemRepository : IStockItemRepository
    {
        private readonly DbContext _db;
        private readonly DbSet<StockItem> _dbSet;
        public StockItemRepository(DbContext db)
        {
            _db = db;
            _dbSet = db.Set<StockItem>();
        }

        public async Task<IList<StockItem>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<StockItem> GetByIdAsync(string Isin) => await _dbSet.FindAsync(Isin);

        public void Insert(StockItem entity) => _dbSet.Add(entity);

        public void Update(StockItem entity, decimal? price, int? quantity)
        {
            if (entity == null)
                throw new InvalidOperationException("Entity cannot be null.");

            if (price.HasValue)
                entity.Price = price.Value;

            if (quantity.HasValue)
                entity.Quantity = quantity.Value;
        }

        public void Delete(StockItem entity) => _dbSet.Remove(entity);

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public IQueryable<StockItem> Find(Expression<Func<StockItem, bool>> predicate) => _dbSet.Where(predicate);

    }
}
