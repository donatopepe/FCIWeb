using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FCIWeb.Web.Tests
{
    public sealed class CsvExportWebTests
    {
        [Fact]
        public async Task ExportCsv_RequiresAuthentication()
        {
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseSetting("FCIWEB_STORE", "memory"));
            using var client = TestClient.Create(factory);
            var response = await client.GetAsync("/Richieste/ExportCsv");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task ExportXlsx_RequiresAuthentication()
        {
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseSetting("FCIWEB_STORE", "memory"));
            using var client = TestClient.Create(factory);
            var response = await client.GetAsync("/Richieste/ExportXlsx");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task ExportXlsx_UsesSearchFilterAndAttachmentHeaders()
        {
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseSetting("FCIWEB_STORE", "memory"));
            using var client = TestClient.Create(factory);
            await TestClient.LoginAsync(client, "user", "User123!");

            var response = await client.GetAsync("/Richieste/ExportXlsx?q=1001");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", response.Content.Headers.ContentType.MediaType);
            Assert.Contains(".xlsx", response.Content.Headers.ContentDisposition.ToString());
            var bytes = await response.Content.ReadAsByteArrayAsync();
            using var stream = new System.IO.MemoryStream(bytes);
            using var workbook = DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Open(stream, false);
            var rows = workbook.WorkbookPart.WorksheetParts.Single().Worksheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.SheetData>().Elements<DocumentFormat.OpenXml.Spreadsheet.Row>().ToArray();
            Assert.Equal(2, rows.Length);
            Assert.Contains("1001", rows[1].InnerText);
            Assert.DoesNotContain("1002", rows[1].InnerText);
        }

        [Fact]
        public async Task ExportCsv_UsesSearchFilterAndAttachmentHeaders()
        {
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseSetting("FCIWEB_STORE", "memory"));
            using var client = TestClient.Create(factory);
            await TestClient.LoginAsync(client, "user", "User123!");

            var response = await client.GetAsync("/Richieste/ExportCsv?q=1001");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/csv", response.Content.Headers.ContentType.MediaType);
            Assert.Contains("attachment", response.Content.Headers.ContentDisposition.ToString());
            var csv = await response.Content.ReadAsStringAsync();
            Assert.Contains("1001", csv);
            Assert.DoesNotContain("1002", csv);
        }
    }
}
