using System;
using System.Collections.Generic;
using System.Linq;

namespace FCIWeb.Core.Security
{
    /// <summary>Impostazioni SMTP usate dalla validazione di sicurezza.</summary>
    public class SecureSmtpOptions
    {
        public string Host { get; set; }
        public int Port { get; set; } = 587;
        public bool UseSsl { get; set; } = true;
        public string Username { get; set; }
        public string Password { get; set; }

        /// <summary>Se true non viene inviata posta reale (modalità demo/test).</summary>
        public bool IsSimulated { get; set; }
    }

    /// <summary>Errore di configurazione SMTP non sicura.</summary>
    public class InsecureSmtpConfigurationException : Exception
    {
        public InsecureSmtpConfigurationException(IReadOnlyList<string> errors)
            : base("Configurazione SMTP non sicura: " + String.Join(" | ", errors))
        {
            Errors = errors;
        }

        public IReadOnlyList<string> Errors { get; }
    }

    /// <summary>
    /// Validazione della configurazione SMTP: impedisce invio di credenziali in chiaro su host
    /// remoti, porte invalide e credenziali incoerenti.
    /// </summary>
    public static class SecureSmtpValidator
    {
        private static readonly string[] LoopbackHosts =
        {
            "localhost", "127.0.0.1", "::1", "[::1]", "0.0.0.0"
        };

        public static bool IsLoopback(string host)
            => !String.IsNullOrWhiteSpace(host)
               && LoopbackHosts.Contains(host.Trim(), StringComparer.OrdinalIgnoreCase);

        /// <summary>Elenca i problemi rilevati; lista vuota = configurazione accettabile.</summary>
        public static IReadOnlyList<string> Validate(SecureSmtpOptions options)
        {
            var errors = new List<string>();
            if (options == null)
            {
                errors.Add("Configurazione SMTP assente.");
                return errors;
            }

            if (options.Port < 1 || options.Port > 65535)
                errors.Add($"Porta SMTP fuori intervallo: {options.Port}.");

            if (options.IsSimulated)
                return errors;

            if (String.IsNullOrWhiteSpace(options.Host))
                errors.Add("Host SMTP mancante.");

            var hasUsername = !String.IsNullOrWhiteSpace(options.Username);
            var hasPassword = !String.IsNullOrWhiteSpace(options.Password);

            if (hasPassword && !hasUsername)
                errors.Add("Password SMTP presente senza username.");
            if (hasUsername && !hasPassword)
                errors.Add("Username SMTP presente senza password.");

            if (hasUsername && !options.UseSsl && !IsLoopback(options.Host))
                errors.Add("Credenziali SMTP inviate in chiaro su host remoto: abilitare SSL/TLS.");

            return errors;
        }

        /// <summary>Solleva <see cref="InsecureSmtpConfigurationException"/> se la configurazione non è sicura.</summary>
        public static void EnsureValid(SecureSmtpOptions options)
        {
            var errors = Validate(options);
            if (errors.Count > 0)
                throw new InsecureSmtpConfigurationException(errors);
        }
    }
}
