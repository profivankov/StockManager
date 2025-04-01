using StockManager.Core.DTOs;
using StockManager.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StockManager.Infrastructure.Interfaces
{
	public interface IStockItemRepository
	{
		Task<IList<StockItem>> GetAllAsync();
		Task<StockItem> GetByIdAsync(string Isin);
		void InsertAsync(StockItem entity);
		void Update(StockItem entity, StockItemDTO dto);
		void Delete(StockItem entity);
		Task SaveAsync();
		IQueryable<StockItem> Find(Expression<Func<StockItem, bool>> predicate);
	}
}
