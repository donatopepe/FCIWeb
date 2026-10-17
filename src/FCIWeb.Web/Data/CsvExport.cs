using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using FCIWeb.Core.Models;

namespace FCIWeb.Web.Data
{
    /// <summary>CSV export with RFC-style field quoting and spreadsheet formula-injection mitigation.</summary>
    public static class CsvExport
    {
        private static readonly string[] Headers =
        {
            "Id", "NDG", "Intestazione", "UO", "Matricola", "DataCreazioneUtc", "Stato", "Note"
        };

        /// <summary>Serializes records to UTF-8 CSV with BOM for common spreadsheet compatibility.</summary>
        public static byte[] Serialize(IEnumerable<Richiesta> richieste)
        {
            var builder = new StringBuilder();
            AppendRow(builder, Headers);
            foreach (var richiesta in richieste ?? Enumerable.Empty<Richiesta>())
            {
                if (richiesta == null) continue;
                AppendRow(builder, new[]
                {
                    richiesta.Id.ToString("D"),
                    richiesta.NDG,
                    richiesta.Intestazione,
                    richiesta.UOCode,
                    richiesta.Matricola,
                    richiesta.DataCreazione.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                    richiesta.Stato.ToString(),
                    richiesta.Note
                });
            }

            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            var body = encoding.GetBytes(builder.ToString());
            var preamble = encoding.GetPreamble();
            var result = new byte[preamble.Length + body.Length];
            Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
            Buffer.BlockCopy(body, 0, result, preamble.Length, body.Length);
            return result;
        }

        private static void AppendRow(StringBuilder builder, IEnumerable<string> fields)
        {
            var first = true;
            foreach (var field in fields)
            {
                if (!first) builder.Append(',');
                first = false;
                builder.Append(EscapeField(field));
            }
            builder.Append("\r\n");
        }

        private static string EscapeField(string value)
        {
            value ??= String.Empty;
            value = NeutralizeSpreadsheetFormula(value);
            var escaped = value.Replace("\"", "\"\"");
            return "\"" + escaped + "\"";
        }

        /// <summary>
        /// Prefixes a single quote when a spreadsheet could interpret untrusted cell content as
        /// a formula. Leading control/whitespace is checked to cover common CSV injection variants.
        /// </summary>
        public static string NeutralizeSpreadsheetFormula(string value)
        {
            if (String.IsNullOrEmpty(value)) return value ?? String.Empty;
            var index = 0;
            while (index < value.Length && (Char.IsWhiteSpace(value[index]) || Char.IsControl(value[index]))) index++;
            if (index >= value.Length) return value;

            var first = value[index];
            return first == '=' || first == '+' || first == '-' || first == '@'
                ? "'" + value
                : value;
        }
    }
}
