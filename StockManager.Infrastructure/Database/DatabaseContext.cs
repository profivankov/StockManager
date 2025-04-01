using StockManager.Core.Entities;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.SQLite;
using System.Data.SQLite.EF6;

namespace StockManager.Infrastructure.Database
{
    public class SQLiteConfiguration : DbConfiguration
    {
        public SQLiteConfiguration()
        {
            SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
            SetProviderServices("System.Data.SQLite", (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
        }
    }
    public class DatabaseContext : DbContext
    {
        public DbSet<StockItem> StockItems { get; set; }

        public DatabaseContext(string connectionString)
            : base(new SQLiteConnection(connectionString), contextOwnsConnection: true) {
            DbConfiguration.SetConfiguration(new SQLiteConfiguration());
        }
    }
}
