using System;
using System.Linq;
using FCIWeb.Core.Models;
using FCIWeb.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FCIWeb.Data.Tests
{
    public sealed class MigrationTests
    {
        [Fact]
        public void Migrate_EmptyDatabaseCreatesSchemaAndIsIdempotent()
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<FCIWebContext>().UseSqlite(connection).Options;

            using (var db = new FCIWebContext(options))
            {
                Assert.Empty(db.Database.GetAppliedMigrations());
                db.Database.Migrate();
                Assert.Single(db.Database.GetAppliedMigrations());
                Assert.Contains("Richieste", db.Model.GetEntityTypes().Select(e => e.GetTableName()));
            }

            using (var db = new FCIWebContext(options))
            {
                db.Database.Migrate();
                db.Richieste.Add(new Richiesta
                {
                    Id = Guid.NewGuid(), NDG = "MIG-1", Intestazione = "Preserved row",
                    DataCreazione = DateTime.UtcNow, Stato = StatoRichiesta.Bozza
                });
                db.SaveChanges();
            }

            using (var reopened = new FCIWebContext(options))
            {
                reopened.Database.Migrate();
                var row = reopened.Richieste.Single(r => r.NDG == "MIG-1");
                Assert.Equal("Preserved row", row.Intestazione);
                Assert.Single(reopened.Database.GetAppliedMigrations());
            }
        }

        [Fact]
        public void AdoptIfNeeded_EnsureCreatedDatabaseRecordsInitialMigrationAndPreservesRows()
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<FCIWebContext>().UseSqlite(connection).Options;
            var row = new Richiesta
            {
                Id = Guid.NewGuid(), NDG = "LEGACY-1", Intestazione = "Existing demo data",
                DataCreazione = DateTime.UtcNow, Stato = StatoRichiesta.Bozza
            };
            // Simulate the exact pre-migrations schema (without Version or migration history).
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE Richieste (Id TEXT NOT NULL PRIMARY KEY, NDG TEXT NOT NULL, Intestazione TEXT NOT NULL, UOCode TEXT NULL, Matricola TEXT NULL, DataCreazione TEXT NOT NULL, Stato INTEGER NOT NULL, Note TEXT NULL)";
                command.ExecuteNonQuery();
                command.CommandText = "INSERT INTO Richieste (Id, NDG, Intestazione, UOCode, Matricola, DataCreazione, Stato, Note) VALUES ($id, $ndg, $title, NULL, NULL, $date, 0, NULL)";
                command.Parameters.AddWithValue("$id", row.Id.ToString());
                command.Parameters.AddWithValue("$ndg", row.NDG);
                command.Parameters.AddWithValue("$title", row.Intestazione);
                command.Parameters.AddWithValue("$date", row.DataCreazione.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
                command.ExecuteNonQuery();
            }

            using (var upgraded = new FCIWebContext(options))
            {
                LegacySchemaAdopter.AdoptIfNeeded(upgraded);
                upgraded.Database.Migrate();
                Assert.Single(upgraded.Database.GetAppliedMigrations());
                Assert.Equal("Existing demo data", upgraded.Richieste.Single(r => r.NDG == "LEGACY-1").Intestazione);
            }
        }

        [Fact]
        public void AdoptIfNeeded_RejectsIncompatibleExistingSchema()
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE Richieste (Id TEXT PRIMARY KEY, NDG TEXT NOT NULL)";
                command.ExecuteNonQuery();
            }
            var options = new DbContextOptionsBuilder<FCIWebContext>().UseSqlite(connection).Options;
            using var db = new FCIWebContext(options);
            Assert.Throws<InvalidOperationException>(() => LegacySchemaAdopter.AdoptIfNeeded(db));
        }
    }
}
