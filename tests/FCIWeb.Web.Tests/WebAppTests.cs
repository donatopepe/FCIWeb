using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FCIWeb.Web.Tests
{
    public class WebAppTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public WebAppTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder => builder.UseSetting("FCIWEB_STORE", "memory"));
        }

        [Fact]
        public async Task Root_Unauthenticated_RedirectsToLogin()
        {
            using var client = TestClient.Create(_factory);
            var response = await client.GetAsync("/");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task LoginPage_IsReachable()
        {
            using var client = TestClient.Create(_factory);
            var response = await client.GetAsync("/Account/Login");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Accesso", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Login_WithValidCredentials_RedirectsToRichieste()
        {
            using var client = TestClient.Create(_factory);
            var response = await TestClient.LoginAsync(client, "admin", "Admin123!");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Richieste", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShowsError()
        {
            using var client = TestClient.Create(_factory);
            var response = await TestClient.LoginAsync(client, "admin", "wrong-password");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Credenziali non valide", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Richieste_RequiresAuthentication()
        {
            using var client = TestClient.Create(_factory);
            var response = await client.GetAsync("/Richieste");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Fact]
        public async Task Richieste_AfterLogin_ShowsSeededData()
        {
            using var client = TestClient.Create(_factory);
            await TestClient.LoginAsync(client, "user", "User123!");
            var response = await client.GetAsync("/Richieste");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var html = await response.Content.ReadAsStringAsync();
            Assert.Contains("1001", html);
            Assert.Contains("1002", html);
        }

        [Fact]
        public async Task Create_WithoutAntiforgeryToken_IsRejected()
        {
            using var client = TestClient.Create(_factory);
            await TestClient.LoginAsync(client, "user", "User123!");
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("NDG", "9999"),
                new KeyValuePair<string, string>("Intestazione", "senza token")
            });
            var response = await client.PostAsync("/Richieste/Create", content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithAntiforgeryToken_CreatesAndRedirects()
        {
            using var client = TestClient.Create(_factory);
            await TestClient.LoginAsync(client, "user", "User123!");

            var token = await TestClient.GetAntiforgeryTokenAsync(client, "/Richieste/Create");
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("NDG", "3001"),
                new KeyValuePair<string, string>("Intestazione", "Integrazione"),
                new KeyValuePair<string, string>("UOCode", "UO009"),
                new KeyValuePair<string, string>("__RequestVerificationToken", token)
            });
            var response = await client.PostAsync("/Richieste/Create", content);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Richieste/Details/", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task SecurityHeaders_ArePresent()
        {
            using var client = TestClient.Create(_factory);
            var response = await client.GetAsync("/Account/Login");

            Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());
            Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").First());
            Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").First());
            Assert.True(response.Headers.Contains("Content-Security-Policy"));
        }
    }

    internal static class HeaderExtensions
    {
        public static string First(this IEnumerable<string> values)
        {
            foreach (var v in values) return v;
            return null;
        }
    }
}
