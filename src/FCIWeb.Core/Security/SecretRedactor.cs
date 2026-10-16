using System;
using System.Text.RegularExpressions;

namespace FCIWeb.Core.Security
{
    /// <summary>
    /// Maschera i segreti nei testi destinati a log, errori o output diagnostico.
    /// Evita che credenziali finiscano nei log applicativi.
    /// </summary>
    public static class SecretRedactor
    {
        public const string Mask = "***REDACTED***";

        private static readonly Regex[] Patterns =
        {
            // password=..., pwd: ..., Password = "..."
            new Regex(@"(?i)\b(password|pwd|passwd)\b\s*[:=]\s*(""[^""]*""|'[^']*'|[^;,\s]+)", RegexOptions.Compiled),
            // User ID=...;Password=... in connection string
            new Regex(@"(?i)\b(user\s*id|uid|username)\b\s*=\s*([^;""']+)", RegexOptions.Compiled),
            // api key / token / secret
            new Regex(@"(?i)\b(api[_-]?key|access[_-]?token|refresh[_-]?token|client[_-]?secret|secret)\b\s*[:=]\s*(""[^""]*""|'[^']*'|[^;,\s]+)", RegexOptions.Compiled),
            // Authorization: Bearer <token>
            new Regex(@"(?i)\b(bearer)\s+[A-Za-z0-9\-\._~\+/]+=*", RegexOptions.Compiled),
            // chiavi private
            new Regex(@"(?is)-----BEGIN [A-Z ]*PRIVATE KEY-----.*?-----END [A-Z ]*PRIVATE KEY-----", RegexOptions.Compiled),
        };

        /// <summary>Restituisce il testo con i segreti mascherati.</summary>
        public static string Redact(string text)
        {
            if (String.IsNullOrEmpty(text)) return text;

            var result = text;
            foreach (var pattern in Patterns)
            {
                result = pattern.Replace(result, match =>
                {
                    // Mantiene l'etichetta, maschera solo il valore.
                    var lastGroup = match.Groups.Count - 1;
                    if (match.Groups.Count > 1 && match.Groups[lastGroup].Success && match.Groups[lastGroup].Length > 0)
                    {
                        return match.Value.Substring(0, match.Groups[lastGroup].Index - match.Index) + Mask;
                    }
                    return Mask;
                });
            }
            return result;
        }

        /// <summary>Maschera una connection string ADO.NET mantenendone la struttura.</summary>
        public static string RedactConnectionString(string connectionString)
            => Redact(connectionString);
    }
}
