using StockManager.Core.DTOs;
using StockManager.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManager.App.Interfaces
{
    public interface IStockService
    {
        Task AddStock(StockItemDTO dto);
        Task UpdateStock(StockItemDTO dto);
        Task<IList<StockItem>> ListAllStocks();
        IQueryable<StockItem> QueryBelowThreshold(int threshold);
        IQueryable<StockItem> Search(string Isin = null, string partialName = null);
    }
}
