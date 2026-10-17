using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FCIWeb.Web.Tests
{
    /// <summary>Helper per i test di integrazione: client con cookie e token antiforgery.</summary>
    public static class TestClient
    {
        private static readonly Regex TokenRegex =
            new Regex("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"", RegexOptions.Compiled);

        public static HttpClient Create(WebApplicationFactory<Program> factory)
        {
            return factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
                BaseAddress = new System.Uri("https://localhost")
            });
        }

        /// <summary>Esegue il login e restituisce la risposta (302 in caso di successo).</summary>
        public static Task<HttpResponseMessage> LoginAsync(HttpClient client, string username, string password)
            => LoginAsync(client, username, password, null);

        public static async Task<HttpResponseMessage> LoginAsync(HttpClient client, string username, string password, string returnUrl)
        {
            var url = "/Account/Login" + (String.IsNullOrEmpty(returnUrl) ? String.Empty : "?returnUrl=" + Uri.EscapeDataString(returnUrl));
            var token = await GetAntiforgeryTokenAsync(client, url);
            var content = new FormUrlEncodedContent(new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string>("Username", username),
                new System.Collections.Generic.KeyValuePair<string, string>("Password", password),
                new System.Collections.Generic.KeyValuePair<string, string>("__RequestVerificationToken", token)
            });
            return await client.PostAsync("/Account/Login", content);
        }

        /// <summary>Estrae il token antiforgery nascosto da una pagina GET.</summary>
        public static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var match = TokenRegex.Match(html);
            if (!match.Success) throw new InvalidOperationException($"Token antiforgery non trovato in {url}");
            return match.Groups[1].Value;
        }
    }
}
