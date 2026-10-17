using System;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using FCIWeb.Core.Models;
using FCIWeb.Web.Data;
using Xunit;

namespace FCIWeb.Web.Tests
{
    public sealed class XlsxExportTests
    {
        [Fact]
        public void Serialize_CreatesReadableWorkbookWithInlineUntrustedText()
        {
            var bytes = XlsxExport.Serialize(new[]
            {
                new Richiesta { Id = Guid.NewGuid(), NDG = "=1+1", Intestazione = "A & <B>",
                    UOCode = "UO", Matricola = "M1", DataCreazione = DateTime.UtcNow,
                    Stato = StatoRichiesta.Bozza, Note = "note" }
            });
            using var stream = new MemoryStream(bytes);
            using var document = SpreadsheetDocument.Open(stream, false);
            var sheet = document.WorkbookPart.Workbook.Sheets.Elements<Sheet>().Single();
            Assert.Equal("Richieste", sheet.Name.Value);
            var worksheet = (WorksheetPart)document.WorkbookPart.GetPartById(sheet.Id.Value);
            var rows = worksheet.Worksheet.GetFirstChild<SheetData>().Elements<Row>().ToArray();
            Assert.Equal(2, rows.Length);
            Assert.Equal("Id", rows[0].Elements<Cell>().First().InlineString.Text.Text);
            var values = rows[1].Elements<Cell>().Select(cell => cell.InlineString?.Text?.Text).ToArray();
            Assert.Equal("=1+1", values[1]);
            Assert.Equal("A & <B>", values[2]);
            Assert.All(rows[1].Elements<Cell>(), cell => Assert.Equal(CellValues.InlineString, cell.DataType.Value));
        }

        [Fact]
        public void Serialize_EmptyCollectionCreatesValidHeaderOnlyWorkbook()
        {
            var bytes = XlsxExport.Serialize(Array.Empty<Richiesta>());
            using var stream = new MemoryStream(bytes);
            using var document = SpreadsheetDocument.Open(stream, false);
            var worksheet = document.WorkbookPart.WorksheetParts.Single().Worksheet;
            Assert.Single(worksheet.GetFirstChild<SheetData>().Elements<Row>());
        }
    }
}
