using System;
using System.Linq;
using FCIWeb.Core.Models;
using FCIWeb.Data;
using Xunit;

namespace FCIWeb.Data.Tests
{
    [Collection("SQLite")]
    public sealed class PagingRepositoryTests
    {
        private readonly SqliteFixture _fixture;
        public PagingRepositoryTests(SqliteFixture fixture) => _fixture = fixture;

        private sealed class PageData
        {
            public System.Collections.Generic.IReadOnlyList<Richiesta> Items { get; init; }
            public int PageNumber { get; init; }
            public int PageSize { get; init; }
            public int TotalCount { get; init; }
            public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
            public bool HasPreviousPage => PageNumber > 1 && TotalPages > 0;
            public bool HasNextPage => PageNumber < TotalPages;
        }

        private static PageData Page(EfRichiestaRepository repo, string query, int number, int size)
        {
            var result = repo.GetPage(query, number, size);
            var safeSize = Math.Clamp(size, 1, 50);
            var totalPages = result.TotalCount == 0 ? 0 : (int)Math.Ceiling(result.TotalCount / (double)safeSize);
            var page = totalPages == 0 ? 1 : Math.Min(Math.Max(1, number), totalPages);
            return new PageData { Items = result.Items, PageNumber = page, PageSize = safeSize, TotalCount = result.TotalCount };
        }

        [Fact]
        public void GetPage_ReturnsStableNonOverlappingPages()
        {
            using var db = _fixture.CreateContext();
            var repo = new EfRichiestaRepository(db);
            var anchor = DateTime.UtcNow.AddMinutes(-1);
            for (var i = 0; i < 23; i++)
            {
                repo.Add(new Richiesta
                {
                    Id = Guid.NewGuid(), NDG = $"PAGE-{i:D2}", Intestazione = $"Paging sample {i:D2}",
                    UOCode = "PAGE-UO", DataCreazione = anchor, Stato = StatoRichiesta.Bozza
                });
            }

            var first = Page(repo, "PAGE-", 1, 10);
            var second = Page(repo, "PAGE-", 2, 10);
            var last = Page(repo, "PAGE-", 3, 10);

            Assert.Equal(23, first.TotalCount);
            Assert.Equal(3, first.TotalPages);
            Assert.Equal(10, first.Items.Count);
            Assert.Equal(10, second.Items.Count);
            Assert.Equal(3, last.Items.Count);
            Assert.True(first.HasNextPage);
            Assert.False(first.HasPreviousPage);
            Assert.True(second.HasPreviousPage);
            Assert.True(second.HasNextPage);
            Assert.Equal(23, first.Items.Select(x => x.Id).Concat(second.Items.Select(x => x.Id)).Concat(last.Items.Select(x => x.Id)).Distinct().Count());
        }

        [Fact]
        public void GetPage_NormalizesInvalidPageAndCapsPageSize()
        {
            using var db = _fixture.CreateContext();
            var repo = new EfRichiestaRepository(db);
            for (var i = 0; i < 3; i++)
            {
                repo.Add(new Richiesta
                {
                    Id = Guid.NewGuid(), NDG = $"NORM-{i}", Intestazione = $"Normalize {i}",
                    DataCreazione = DateTime.UtcNow.AddSeconds(i), Stato = StatoRichiesta.Bozza
                });
            }

            var zero = Page(repo, "NORM-", 0, 0);
            Assert.Single(zero.Items);
            Assert.Equal(1, zero.PageNumber);
            Assert.Equal(1, zero.PageSize);

            var beyond = Page(repo, "NORM-", 99, 500);
            Assert.Equal(3, beyond.TotalCount);
            Assert.Equal(1, beyond.TotalPages);
            Assert.Equal(1, beyond.PageNumber);
            Assert.Equal(50, beyond.PageSize);
            Assert.Equal(3, beyond.Items.Count);
        }

        [Fact]
        public void GetPage_EmptySearchReturnsEmptyPageOne()
        {
            using var db = _fixture.CreateContext();
            var page = Page(new EfRichiestaRepository(db), "NO-MATCH-PAGING", 4, 10);
            Assert.Empty(page.Items);
            Assert.Equal(0, page.TotalCount);
            Assert.Equal(0, page.TotalPages);
            Assert.Equal(1, page.PageNumber);
        }
    }
}
