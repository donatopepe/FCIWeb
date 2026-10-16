using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FCIWeb.Web.Tests
{
    public sealed class PersistentWebTests
    {
        [Fact]
        public async Task SqliteStore_CreateRequestPersistsAcrossHttpRequests()
        {
            var database = Path.Combine(Path.GetTempPath(), "fciweb-test-" + Guid.NewGuid().ToString("N") + ".db");
            try
            {
                using var factory = new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(builder => builder.UseSetting("FCIWEB_SQLITE_PATH", database));
                using var client = TestClient.Create(factory);

                var login = await TestClient.LoginAsync(client, "user", "User123!");
                Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

                var token = await TestClient.GetAntiforgeryTokenAsync(client, "/Richieste/Create");
                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("NDG", "PERSIST-1"),
                    new KeyValuePair<string, string>("Intestazione", "Persistente via HTTP"),
                    new KeyValuePair<string, string>("UOCode", "UO-PERSIST"),
                    new KeyValuePair<string, string>("__RequestVerificationToken", token)
                });
                var created = await client.PostAsync("/Richieste/Create", form);
                Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);

                var response = await client.GetAsync("/Richieste?q=PERSIST-1");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var html = await response.Content.ReadAsStringAsync();
                Assert.Contains("PERSIST-1", html);
                Assert.Contains("Persistente via HTTP", html);
                Assert.True(File.Exists(database));
            }
            finally
            {
                foreach (var path in new[] { database, database + "-shm", database + "-wal" })
                    if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
