using System;
using System.Collections.Generic;
using System.Text;
using FCIWeb.Core.Models;
using FCIWeb.Web.Data;
using Xunit;

namespace FCIWeb.Web.Tests
{
    public sealed class CsvExportTests
    {
        [Fact]
        public void Serialize_QuotesCommasQuotesAndNewlines()
        {
            var csv = Encoding.UTF8.GetString(CsvExport.Serialize(new[]
            {
                new Richiesta
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), NDG = "12,34",
                    Intestazione = "titolo \"quoted\"\nseconda riga", UOCode = "UO-1",
                    Matricola = "M1", DataCreazione = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                    Stato = StatoRichiesta.Bozza, Note = "note"
                }
            }));

            Assert.StartsWith("\uFEFF\"Id\",\"NDG\"", csv);
            Assert.Contains("\"12,34\"", csv);
            Assert.Contains("\"titolo \"\"quoted\"\"\nseconda riga\"", csv);
            Assert.Contains("\"2026-01-02T03:04:05.0000000Z\"", csv);
        }

        [Theory]
        [InlineData("=1+1", "'=1+1")]
        [InlineData("+cmd|' /C calc'!A0", "'+cmd|' /C calc'!A0")]
        [InlineData("-10+20", "'-10+20")]
        [InlineData("@SUM(A1:A2)", "'@SUM(A1:A2)")]
        [InlineData(" \t=HYPERLINK(\"https://bad\")", "' \t=HYPERLINK(\"https://bad\")")]
        [InlineData("ordinary text", "ordinary text")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void NeutralizeSpreadsheetFormula_HandlesDangerousPrefixes(string input, string expected)
        {
            Assert.Equal(expected, CsvExport.NeutralizeSpreadsheetFormula(input));
        }

        [Fact]
        public void Serialize_EmptyCollectionProducesHeaderOnly()
        {
            var csv = Encoding.UTF8.GetString(CsvExport.Serialize(Array.Empty<Richiesta>()));
            Assert.Equal("\uFEFF\"Id\",\"NDG\",\"Intestazione\",\"UO\",\"Matricola\",\"DataCreazioneUtc\",\"Stato\",\"Note\"\r\n", csv);
        }
    }
}
