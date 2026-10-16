using System;
using FCIWeb.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace FCIWeb.Data.Tests
{
    public sealed class SqlServerMigrationScriptTests
    {
        [Fact]
        public void SqlServerProvider_GeneratesInitialMigrationScriptWithoutConnecting()
        {
            var options = new DbContextOptionsBuilder<FCIWebContext>()
                .UseSqlServer("Server=localhost;Database=FCIWebMigrationDesign;Trusted_Connection=True;TrustServerCertificate=True")
                .Options;
            using var context = new FCIWebContext(options);
            var migrator = context.GetService<IMigrator>();

            // GenerateScript uses the provider's SQL generator and migration metadata only;
            // it must not open a connection or require a live SQL Server.
            var script = migrator.GenerateScript(
                fromMigration: Migration.InitialDatabase,
                toMigration: null,
                options: MigrationsSqlGenerationOptions.Idempotent);

            Assert.Contains("CREATE TABLE [Richieste]", script, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("__EFMigrationsHistory", script, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("CREATE INDEX", script, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Version", script, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("GO", script, StringComparison.OrdinalIgnoreCase);
        }
    }
}
