using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FCIWeb.Data
{
    /// <summary>
    /// Design-time factory for EF CLI. Defaults to an isolated SQLite file; production tooling can
    /// provide a provider/connection via environment variables, never checked-in credentials.
    /// </summary>
    public sealed class FCIWebContextFactory : IDesignTimeDbContextFactory<FCIWebContext>
    {
        public FCIWebContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<FCIWebContext>();
            var provider = System.Environment.GetEnvironmentVariable("FCIWEB_EF_PROVIDER");
            if (string.Equals(provider, "SqlServer", System.StringComparison.OrdinalIgnoreCase))
            {
                var connection = System.Environment.GetEnvironmentVariable("FCIWEB_SQLSERVER_CONNECTION");
                if (string.IsNullOrWhiteSpace(connection))
                    throw new System.InvalidOperationException("FCIWEB_SQLSERVER_CONNECTION must be set for SQL Server design-time operations.");
                options.UseSqlServer(connection);
            }
            else
            {
                var path = System.Environment.GetEnvironmentVariable("FCIWEB_SQLITE_PATH") ?? "data/fciweb.db";
                options.UseSqlite($"Data Source={path}");
            }

            return new FCIWebContext(options.Options);
        }
    }
}
