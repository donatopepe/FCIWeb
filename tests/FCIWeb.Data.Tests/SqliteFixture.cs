using System;
using FCIWeb.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FCIWeb.Data.Tests
{
    public sealed class SqliteFixture : IDisposable
    {
        private readonly SqliteConnection _connection;
        public DbContextOptions<FCIWebContext> Options { get; }

        public SqliteFixture()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            Options = new DbContextOptionsBuilder<FCIWebContext>().UseSqlite(_connection).Options;
            using var db = new FCIWebContext(Options);
            db.Database.Migrate();
        }

        public FCIWebContext CreateContext() => new FCIWebContext(Options);

        public void Dispose() => _connection.Dispose();
    }

    [CollectionDefinition("SQLite", DisableParallelization = true)]
    public sealed class SqliteTestGroup : ICollectionFixture<SqliteFixture> { }
}
