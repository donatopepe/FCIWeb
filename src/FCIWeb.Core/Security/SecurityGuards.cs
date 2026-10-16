using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FCIWeb.Core.Security
{
    /// <summary>Errore di validazione di sicurezza (input non attendibile).</summary>
    public class SecurityGuardException : Exception
    {
        public SecurityGuardException(string message) : base(message) { }
    }

    /// <summary>
    /// Guardie di sicurezza per input non attendibile: content-type, dimensione payload,
    /// nomi file e percorsi. Cross-platform: tratta i separatori Windows e POSIX su qualsiasi OS,
    /// perché l'input può provenire da client eterogenei.
    /// </summary>
    public static class SecurityGuards
    {
        /// <summary>Limite predefinito per il corpo JSON (1 MiB).</summary>
        public const long DefaultMaxJsonBodyBytes = 1024 * 1024;

        private static readonly char[] Separators = { '/', '\\' };

        private static readonly HashSet<char> ExtraInvalidFileNameChars =
            new HashSet<char> { '<', '>', ':', '"', '|', '?', '*' };

        private static readonly HashSet<string> WindowsReservedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        /// <summary>
        /// Verifica se un content-type è JSON, senza eccezioni su valore nullo.
        /// Il legacy usava <c>contentType.Contains(...)</c> che andava in NullReferenceException.
        /// </summary>
        public static bool IsJsonContentType(string contentType)
            => !String.IsNullOrWhiteSpace(contentType)
               && contentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0;

        /// <summary>Solleva <see cref="SecurityGuardException"/> se la dimensione supera il limite.</summary>
        public static void EnsureWithinSize(long length, long maxBytes, string name = "payload")
        {
            if (maxBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maxBytes));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (length > maxBytes)
                throw new SecurityGuardException($"'{name}' troppo grande: {length} byte (limite {maxBytes}).");
        }

        /// <summary>
        /// Rende sicuro un nome file: prende l'ultimo segmento (separatori Windows e POSIX),
        /// rimuove caratteri invalidi/riservati e risolve i path traversal.
        /// Restituisce stringa vuota se non resta nulla di valido.
        /// </summary>
        public static string SanitizeFileName(string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName)) return String.Empty;

            var trimmed = fileName.Trim();
            var lastSeparator = trimmed.LastIndexOfAny(Separators);
            var candidate = lastSeparator >= 0 ? trimmed.Substring(lastSeparator + 1) : trimmed;
            if (candidate.Length == 0) return String.Empty;

            var invalid = new HashSet<char>(Path.GetInvalidFileNameChars());
            invalid.UnionWith(ExtraInvalidFileNameChars);

            var cleaned = new string(candidate
                .Select(c => (invalid.Contains(c) || Char.IsControl(c)) ? '_' : c)
                .ToArray())
                .Trim()
                .Trim('.'); // niente file nascosti, "." o ".."

            if (cleaned.Length == 0) return String.Empty;

            var stem = Path.GetFileNameWithoutExtension(cleaned);
            if (WindowsReservedNames.Contains(stem))
            {
                cleaned = "_" + cleaned;
            }
            return cleaned;
        }

        /// <summary>
        /// Vero se il percorso relativo è privo di traversal e non assoluto.
        /// Riconosce: radici POSIX ("/"), UNC ("//", "\\\\"), drive Windows ("C:/", "C:x").
        /// </summary>
        public static bool IsSafeRelativePath(string relativePath)
        {
            if (String.IsNullOrWhiteSpace(relativePath)) return false;

            var normalized = relativePath.Replace('\\', '/').Trim();
            if (normalized.Length == 0) return false;

            if (normalized.StartsWith("/", StringComparison.Ordinal)) return false;   // assoluto POSIX / UNC
            if (normalized.Length >= 2 && Char.IsLetter(normalized[0]) && normalized[1] == ':') return false; // drive
            if (normalized.Contains("..")) return false;                              // traversal

            var segments = normalized.Split('/');
            return segments.All(s => s.Length > 0 && s != ".");
        }

        /// <summary>
        /// Combina un percorso relativo sotto una root, garantendo che il risultato non esca
        /// dalla root (difesa da path traversal).
        /// </summary>
        public static string CombineUnderRoot(string root, string relativePath)
        {
            if (String.IsNullOrWhiteSpace(root)) throw new ArgumentException("root mancante", nameof(root));
            if (!IsSafeRelativePath(relativePath))
                throw new SecurityGuardException($"Percorso relativo non sicuro: '{relativePath}'.");

            var rootFull = Path.GetFullPath(root);
            var combined = Path.GetFullPath(Path.Combine(rootFull, relativePath));
            var prefix = rootFull.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? rootFull
                : rootFull + Path.DirectorySeparatorChar;

            if (!combined.StartsWith(prefix, StringComparison.Ordinal))
                throw new SecurityGuardException("Percorso risolto fuori dalla root consentita.");

            return combined;
        }
    }
}
