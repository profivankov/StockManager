using System;
using NUnit.Framework;
using StockManager.Infrastructure.Database;
using StockManager.Infrastructure.Interfaces;
using StockManager.Infrastructure.Repositories;
using System.Threading.Tasks;
using StockManager.App.Interfaces;
using StockManager.Core.DTOs;
using StockManager.App.Services;
using System.Linq;
using System.IO;

namespace StockManager.Tests
{
    [TestFixture]
    public class StockServiceIntegrationTests
    {
        private DatabaseContext _db;
        private IStockItemRepository _stockRepository;
        private IStockService _stockService;
        private readonly string _dbPath = @"C:\sqlite\test.db";

        [SetUp]
        public async Task Setup()
        {
            DatabaseHelper.InitializeDatabase();

            string testDbConnection = $"Data Source={_dbPath}; Version=3; FailIfMissing=True;";

            _db = new DatabaseContext(testDbConnection);
            _stockRepository = new StockItemRepository(_db);
            _stockService = new StockService(_stockRepository);

            _db.Database.Initialize(force: true);

            await SeedDatabase();
        }

        private async Task SeedDatabase()
        {
            var dto = new StockItemDTO { Isin = "TEST123", Name = "Test Stock", Quantity = 100, Price = 50.00m };
            await _stockService.AddStock(dto);
        }

        [Test]
        public async Task AddStock_Success()
        {
            var dto = new StockItemDTO { Isin = "NEW123", Name = "New Stock", Quantity = 150, Price = 25.00m };
            await _stockService.AddStock(dto);

            var stock = await _stockRepository.GetByIdAsync("NEW123");
            Assert.IsNotNull(stock);
            Assert.AreEqual("New Stock", stock.Name);
        }

        [Test]
        public async Task AddStock_Duplicate_Fails()
        {
            var dto = new StockItemDTO { Isin = "TEST123", Name = "Duplicate Stock", Quantity = 50, Price = 20.00m };
            await _stockService.AddStock(dto);

            var allStocks = await _stockRepository.GetAllAsync();
            Assert.AreEqual(1, allStocks.Count(st => st.Isin == "TEST123"));
        }

        [Test]
        public async Task UpdateStock_Success()
        {
            var dto = new StockItemDTO { Isin = "TEST123", Quantity = 200, Price = 75.00m };
            await _stockService.UpdateStock(dto);

            var updatedStock = await _stockRepository.GetByIdAsync("TEST123");
            Assert.AreEqual(200, updatedStock.Quantity);
            Assert.AreEqual(75.00m, updatedStock.Price);
        }

        [Test]
        public void UpdateStock_Fails_When_Stock_Not_Found()
        {
            var dto = new StockItemDTO { Isin = "NOTEXIST", Quantity = 10, Price = 5.00m };
            var ex = Assert.ThrowsAsync<Exception>(async () => await _stockService.UpdateStock(dto));
            Assert.That(ex.Message, Is.EqualTo("Stock not found for update"));
        }

        [Test]
        public async Task ListAllStocks()
        {
            var stocks = await _stockService.ListAllStocks();
            Assert.IsNotEmpty(stocks);
            Assert.AreEqual(2, stocks.Count);
        }

        [Test]
        public void QueryBelowThreshold()
        {
            var lowStockItems = _stockService.QueryBelowThreshold(150).ToList();
            Assert.AreEqual(1, lowStockItems.Count);
            Assert.AreEqual("TEST123", lowStockItems.First().Isin);
        }

        [Test]
        public void Search_By_Isin()
        {
            var result = _stockService.Search(Isin: "TEST123").ToList();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Test Stock", result.First().Name);
        }

        [Test]
        public void Search_By_Partial_Name()
        {
            var result = _stockService.Search(partialName: "Test").ToList();
            Assert.IsTrue(result.Any());
            Assert.AreEqual("TEST123", result.First().Isin);
        }

        [OneTimeTearDown]
        public void CleanupAfterAllTests()
        {
            if (_db != null)
            {
                _db.Dispose();
            }

            GC.Collect();

            if (File.Exists(_dbPath))
            {
                try
                {
                    File.Delete(_dbPath);
                    Console.WriteLine("Database deleted successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete database: {ex.Message}");
                }
            }
        }

    }
}
