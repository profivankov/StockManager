﻿using StockManager.App.Interfaces;
using StockManager.Core.DTOs;
using StockManager.Core.Entities;
using StockManager.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;

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
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Stock name cannot be empty.");
            }

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

            _stockRepository.Insert(stockItem);
            await _stockRepository.SaveAsync();
        }

        public async Task UpdateStock(StockItemDTO dto)
        {
            var stockItem = await _stockRepository.GetByIdAsync(dto.Isin);

            if (stockItem == null)
            {
                throw new Exception("Stock not found for update");
            }

            _stockRepository.Update(stockItem, dto.Price, dto.Quantity);
            await _stockRepository.SaveAsync();
        }

        public async Task<IList<StockItem>> ListAllStocks() => await _stockRepository.GetAllAsync();

        public IQueryable<StockItem> QueryBelowThreshold(int threshold) => _stockRepository.Find(st => st.Quantity < threshold);

        public IQueryable<StockItem> Search(string Isin = null, string partialName = null)
        {
            IQueryable<StockItem> query = _stockRepository.Find(st => true);
                

            if (Isin != null)
                query = query.Where(st => st.Isin == Isin);

            if (partialName != null)
            {
                var lowerName = partialName.ToLower();
                query = query.Where(st => st.Name.ToLower().Contains(lowerName));
            }

            return query.AsNoTracking();
        }

    }
}
