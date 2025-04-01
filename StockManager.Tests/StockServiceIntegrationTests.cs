using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using StockManager.App.Interfaces;
using StockManager.Core.DTOs;
using StockManager.Infrastructure.Database;
using StockManager.Infrastructure.Interfaces;
using StockManager.Infrastructure.Repositories;
using StockManager.App.Services;
using System.Data.Entity;

namespace StockManager.Tests
{
    public class TestInitializer : DropCreateDatabaseAlways<DatabaseContext>
    {
        protected override void Seed(DatabaseContext context)
        {
            context.Database.ExecuteSqlCommand(
                "CREATE TABLE IF NOT EXISTS StockItems (...)");
        }
    }

    [TestFixture]
    public class StockServiceIntegrationTests
    {
        private DatabaseContext _db;
        private IStockItemRepository _stockRepository;
        private IStockService _stockService;
        private readonly string _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.db");
        private readonly string _connectionString;

        public StockServiceIntegrationTests()
        {
            _connectionString = $"Data Source={_dbPath}; Version=3;";
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            ForceDeleteDatabaseFile();
            CreateFreshDatabase();
        }

        private void CreateFreshDatabase()
        {
            using (var context = new DatabaseContext(_connectionString))
            {
                context.Database.ExecuteSqlCommand(
                    @"CREATE TABLE IF NOT EXISTS StockItems (
                    Isin TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Quantity INTEGER,
                    Price NUMERIC
                )");
            }
        }

        private void ForceDeleteDatabaseFile()
        {
            try
            {
                if (File.Exists(_dbPath))
                {
                    File.Delete(_dbPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete database file: {ex.Message}");
            }
        }

        private async Task SeedInitialData()
        {
            var initialStock = new StockItemDTO
            {
                Isin = "TEST123",
                Name = "Test Stock",
                Quantity = 100,
                Price = 50.00m
            };
            await _stockService.AddStock(initialStock);
        }

        [SetUp]
        public async Task Setup()
        {
            _db = new DatabaseContext(_connectionString);

            _stockRepository = new StockItemRepository(_db);
            _stockService = new StockService(_stockRepository);

            _db.Database.BeginTransaction();

            await SeedInitialData();
        }

        // ------------------------------- ADD STOCK TESTS -------------------------------

        [Test]
        public async Task AddStock_Success()
        {
            var dto = new StockItemDTO { Isin = "NEW123", Name = "New Stock", Quantity = 150, Price = 25.00m };
            await _stockService.AddStock(dto);

            var stock = await _stockRepository.GetByIdAsync("NEW123");
            Assert.IsNotNull(stock);
            Assert.AreEqual("New Stock", stock.Name);
            Assert.AreEqual(150, stock.Quantity);
            Assert.AreEqual(25.00m, stock.Price);
        }

        [Test]
        public async Task AddStock_Duplicate_Isin_ShouldNotInsert()
        {
            var dto = new StockItemDTO { Isin = "TEST123", Name = "Duplicate Stock", Quantity = 50, Price = 20.00m };
            await _stockService.AddStock(dto);

            var count = (await _stockRepository.GetAllAsync()).Count(st => st.Isin == "TEST123");
            Assert.AreEqual(1, count);
        }

        [Test]
        public void AddStock_NullOrEmptyName_ThrowsException()
        {
            var dto = new StockItemDTO { Isin = "INVALID1", Name = null, Quantity = 100, Price = 10.00m };
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _stockService.AddStock(dto));
            Assert.That(ex.Message, Is.EqualTo("Stock name cannot be empty."));
        }

        // ------------------------------- UPDATE STOCK TESTS -------------------------------

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

        // ------------------------------- QUERY & SEARCH TESTS -------------------------------

        [Test]
        public async Task ListAllStocks_ReturnsCorrectCount()
        {
            var stocks = await _stockService.ListAllStocks();
            Assert.IsNotEmpty(stocks);
            Assert.AreEqual(1, stocks.Count);
        }

        [TestCase(150, 1)]
        [TestCase(50, 0)]
        public void QueryBelowThreshold_ReturnsExpectedResults(int threshold, int expectedCount)
        {
            var lowStockItems = _stockService.QueryBelowThreshold(threshold).ToList();
            Assert.AreEqual(expectedCount, lowStockItems.Count);
        }

        [Test]
        public void Search_By_Isin_ReturnsStock()
        {
            var result = _stockService.Search(Isin: "TEST123").ToList();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Test Stock", result.First().Name);
        }

        [Test]
        public void Search_By_Partial_Name_ReturnsMatchingStocks()
        {
            var result = _stockService.Search(partialName: "Test").ToList();
            Assert.IsTrue(result.Any());
            Assert.AreEqual("TEST123", result.First().Isin);
        }

        // ------------------------------- CLEANUP -------------------------------


        [TearDown]
        public void TearDown()
        {
            try
            {
                if (_db.Database.CurrentTransaction != null)
                {
                    _db.Database.CurrentTransaction.Rollback();
                }
                _db.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Teardown error: {ex.Message}");
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DeleteDatabaseFile(retries: 3);
        }

        private void DeleteDatabaseFile(int retries = 1)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    if (File.Exists(_dbPath))
                    {
                        File.Delete(_dbPath);
                        Console.WriteLine("Database file deleted successfully");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    if (i == retries - 1)
                    {
                        Console.WriteLine($"Final delete attempt failed: {ex.Message}");
                        throw;
                    }

                    Console.WriteLine($"Delete failed (attempt {i + 1}): {ex.Message}");
                    Task.Delay(100).Wait();
                }
            }
        }

    }
}
