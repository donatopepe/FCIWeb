using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using FCIWeb.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Globalization;
using Xunit;

namespace FCIWeb.Web.Tests
{
    public sealed class SecurityHardeningTests
    {
        [Fact]
        public async Task Login_RateLimitLocksAccountAfterRepeatedFailures()
        {
            var clock = new ManualClock();
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseSetting("FCIWEB_STORE", "memory");
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<TimeProvider>();
                    services.AddSingleton<TimeProvider>(clock);
                });
            });
            using var client = TestClient.Create(factory);

            for (var i = 0; i < 5; i++)
            {
                var response = await TestClient.LoginAsync(client, "user", "wrong-password");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var body = await response.Content.ReadAsStringAsync();
                Assert.True(body.Contains("Credenziali non valide") || body.Contains("temporaneamente bloccato"), body);
            }

            var locked = await TestClient.LoginAsync(client, "user", "User123!");
            Assert.Equal(HttpStatusCode.OK, locked.StatusCode);
            Assert.Contains("temporaneamente bloccato", await locked.Content.ReadAsStringAsync());

            clock.Advance(TimeSpan.FromMinutes(16));
            var unlocked = await TestClient.LoginAsync(client, "user", "User123!");
            Assert.Equal(HttpStatusCode.Redirect, unlocked.StatusCode);
        }

        [Fact]
        public async Task ExternalReturnUrl_IsNeverRedirected()
        {
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseSetting("FCIWEB_STORE", "memory"));
            using var client = TestClient.Create(factory);
            var response = await TestClient.LoginAsync(client, "user", "User123!", "https://outside.invalid/steal-cookie");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Richieste", response.Headers.Location.ToString());
            Assert.DoesNotContain("outside.invalid", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task Cookie_HasHttpOnlySecureAndSameSiteProtection()
        {
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseSetting("FCIWEB_STORE", "memory"));
            using var client = TestClient.Create(factory);
            var login = await TestClient.LoginAsync(client, "user", "User123!");
            var cookie = login.Headers.GetValues("Set-Cookie").FirstOrDefault(x => x.StartsWith("fciweb.demo=", StringComparison.Ordinal));
            Assert.NotNull(cookie);
            Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);
            var response = await client.GetAsync("/Account/Login");
            Assert.True(response.Headers.Contains("Strict-Transport-Security"));
        }

        private sealed class ManualClock : TimeProvider
        {
            private DateTimeOffset _utcNow = DateTimeOffset.Parse("2026-10-16T12:00:00Z", CultureInfo.InvariantCulture);
            public override DateTimeOffset GetUtcNow() => _utcNow;
            public void Advance(TimeSpan duration) => _utcNow += duration;
        }
    }
}
