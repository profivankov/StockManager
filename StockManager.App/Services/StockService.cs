using StockManager.App.Interfaces;
using StockManager.Core.DTOs;
using StockManager.Core.Entities;
using StockManager.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManager.App.Services
{
    public class StockService : IStockService
    {
        private readonly IStockItemRepository _stockRepository;
        public StockService(IStockItemRepository stockRepository) 
        {
            _stockRepository = stockRepository;
        }

        public async Task AddStock(StockItemDTO dto)
        {
            var stockItem = await _stockRepository.GetByIdAsync(dto.Isin);

            if (stockItem != null)
            {
                Console.Error.WriteLine("Stock with this ISIN already exists");
                return;
            }

            stockItem = new StockItem
            {
                Isin = dto.Isin,
                Name = dto.Name,
                Quantity = dto.Quantity ?? 0,
                Price = dto.Price ?? 0
            };

            _stockRepository.InsertAsync(stockItem);
            await _stockRepository.SaveAsync();
        }

        public async Task UpdateStock(StockItemDTO dto)
        {
            var stockItem = await _stockRepository.GetByIdAsync(dto.Isin);

            if (stockItem == null)
            {
                throw new Exception("Stock not found for update");
            }

            _stockRepository.Update(stockItem, dto);
            await _stockRepository.SaveAsync();
        }

        public async Task<IList<StockItem>> ListAllStocks() => await _stockRepository.GetAllAsync();

        public IQueryable<StockItem> QueryBelowThreshold(int threshold) => _stockRepository.Find(st => st.Quantity < threshold);

        public IQueryable<StockItem> Search(string Isin = null, string partialName = null)
        {
            IQueryable<StockItem> query = null;

            if (Isin != null)
            {
                query = _stockRepository.Find( st => st.Isin == Isin);
            }

            if (partialName != null)
            {
                query = _stockRepository.Find(st => st.Name.ToLower().Contains(partialName.ToLower()));
            }

            return query ?? _stockRepository.Find(st => false);
        }

    }
}
