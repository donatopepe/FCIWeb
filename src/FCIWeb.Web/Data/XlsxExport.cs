using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using FCIWeb.Core.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace FCIWeb.Web.Data
{
    /// <summary>Generates minimal XLSX workbooks with untrusted values encoded as inline strings.</summary>
    public static class XlsxExport
    {
        private static readonly string[] Headers =
        {
            "Id", "NDG", "Intestazione", "UO", "Matricola", "DataCreazioneUtc", "Stato", "Note"
        };

        public static byte[] Serialize(IEnumerable<Richiesta> richieste)
        {
            using var output = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(output, SpreadsheetDocumentType.Workbook, true))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1U,
                    Name = "Richieste"
                });

                uint rowIndex = 1;
                sheetData.Append(CreateRow(rowIndex++, Headers));
                foreach (var item in richieste ?? Enumerable.Empty<Richiesta>())
                {
                    if (item == null) continue;
                    sheetData.Append(CreateRow(rowIndex++, new[]
                    {
                        item.Id.ToString("D"), item.NDG, item.Intestazione, item.UOCode,
                        item.Matricola,
                        item.DataCreazione.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                        item.Stato.ToString(), item.Note
                    }));
                }

                workbookPart.Workbook.Save();
                worksheetPart.Worksheet.Save();
            }
            return output.ToArray();
        }

        private static Row CreateRow(uint index, IEnumerable<string> values)
        {
            var row = new Row { RowIndex = index };
            uint column = 1;
            foreach (var value in values)
            {
                row.Append(CreateInlineStringCell(column++, index, value));
            }
            return row;
        }

        private static Cell CreateInlineStringCell(uint column, uint row, string value)
        {
            return new Cell
            {
                CellReference = GetColumnName(column) + row.ToString(CultureInfo.InvariantCulture),
                DataType = CellValues.InlineString,
                InlineString = new InlineString(new Text(value ?? String.Empty) { Space = SpaceProcessingModeValues.Preserve })
            };
        }

        private static string GetColumnName(uint column)
        {
            var name = String.Empty;
            while (column > 0)
            {
                var remainder = (int)((column - 1) % 26);
                name = (char)('A' + remainder) + name;
                column = (column - 1) / 26;
            }
            return name;
        }
    }
}
