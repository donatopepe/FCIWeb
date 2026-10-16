using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace FCIWeb.Web.Security
{
    /// <summary>
    /// Archivio utenti per la modalità demo. Le password sono verificate con PBKDF2-SHA256
    /// (mai confronto in chiaro, mai hash reversibile).
    /// </summary>
    /// <remarks>
    /// Le credenziali predefinite sono **solo demo** e vanno sovrascritte in produzione tramite
    /// configurazione/secret store (sezione <c>Demo:Users</c> o variabili d'ambiente). Vedi SECURITY.md.
    /// </remarks>
    public sealed class DemoUserStore
    {
        private const int Iterations = 100_000;

        private static readonly string[] AdminRoles = { "admin" };
        private static readonly string[] UserRoles = { "user" };

        private sealed class User
        {
            public string Username;
            public byte[] Salt;
            public byte[] Hash;
            public string[] Roles;
        }

        private readonly Dictionary<string, User> _users = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);

        public DemoUserStore(IConfiguration configuration)
        {
            var configured = configuration?.GetSection("Demo:Users").GetChildren().ToList();
            if (configured != null && configured.Count > 0)
            {
                foreach (var child in configured)
                {
                    var username = child["Username"];
                    var password = child["Password"];
                    if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password)) continue;
                    var roles = (child["Roles"] ?? "user")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    AddUser(username, password, roles);
                }
            }

            if (_users.Count == 0)
            {
                // Credenziali demo predefinite (NON segreti; non riutilizzare in produzione).
                AddUser("admin", "Admin123!", AdminRoles);
                AddUser("user", "User123!", UserRoles);
            }
        }

        private void AddUser(string username, string password, string[] roles)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            _users[username] = new User
            {
                Username = username,
                Salt = salt,
                Hash = HashPassword(password, salt),
                Roles = roles ?? Array.Empty<string>()
            };
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32);
        }

        /// <summary>Verifica le credenziali e restituisce il principal, oppure <c>null</c>.</summary>
        public ClaimsPrincipal Authenticate(string username, string password)
        {
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrEmpty(password)) return null;
            if (!_users.TryGetValue(username.Trim(), out var user)) return null;

            var candidate = HashPassword(password, user.Salt);
            if (!CryptographicOperations.FixedTimeEquals(candidate, user.Hash)) return null;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Name, user.Username)
            };
            claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var identity = new ClaimsIdentity(claims, "DemoCookie");
            return new ClaimsPrincipal(identity);
        }

        public bool Exists(string username) => !String.IsNullOrWhiteSpace(username) && _users.ContainsKey(username.Trim());
    }
}
