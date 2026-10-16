using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace FCIWeb.Data
{
    /// <summary>
    /// Adopts a database previously created via EnsureCreated (schema exists, migration history
    /// absent) by validating the initial table layout and recording the initial migration as applied.
    /// This avoids dropping or recreating existing demo data during the switch to migrations.
    /// </summary>
    public static class LegacySchemaAdopter
    {
        private const string InitialMigration = "20261016163341_InitialCreate";

        public static void AdoptIfNeeded(FCIWebContext context)
        {
            if (!context.Database.IsSqlite() || !context.Database.CanConnect()) return;

            var connection = context.Database.GetDbConnection();
            var openedHere = connection.State != System.Data.ConnectionState.Open;
            if (openedHere) connection.Open();
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Richieste'";
                var hasTable = command.ExecuteScalar() != null;
                if (!hasTable) return;

                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory'";
                var hasHistory = command.ExecuteScalar() != null;
                if (hasHistory)
                {
                    command.CommandText = "SELECT COUNT(*) FROM \"__EFMigrationsHistory\"";
                    var historyCount = System.Convert.ToInt32(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
                    if (historyCount > 0) return; // non-empty migration history is authoritative; don't rewrite it
                    // Empty history table can be left by a previous partial startup after EnsureCreated.
                }

                command.CommandText = "PRAGMA table_info('Richieste')";
                using var reader = command.ExecuteReader();
                var columns = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
                while (reader.Read()) columns.Add(reader.GetString(1));
                var required = new[] { "Id", "NDG", "Intestazione", "UOCode", "Matricola", "DataCreazione", "Stato", "Note" };
                var missing = required.Where(c => !columns.Contains(c)).ToArray();
                if (missing.Length > 0)
                    throw new InvalidOperationException("Database SQLite pre-migrations has incompatible Richieste schema; missing columns: " + String.Join(", ", missing));
                reader.Close();

                using var transaction = connection.BeginTransaction();
                if (!hasHistory)
                {
                    using var create = connection.CreateCommand();
                    create.Transaction = transaction;
                    create.CommandText = "CREATE TABLE \"__EFMigrationsHistory\" (\"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY, \"ProductVersion\" TEXT NOT NULL)";
                    create.ExecuteNonQuery();
                }
                using var stamp = connection.CreateCommand();
                stamp.Transaction = transaction;
                stamp.CommandText = "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES (@id, @version)";
                var id = stamp.CreateParameter(); id.ParameterName = "@id"; id.Value = InitialMigration; stamp.Parameters.Add(id);
                var version = stamp.CreateParameter(); version.ParameterName = "@version"; version.Value = "8.0.11"; stamp.Parameters.Add(version);
                stamp.ExecuteNonQuery();
                transaction.Commit();
            }
            finally
            {
                if (openedHere) connection.Close();
            }
        }
    }
}
