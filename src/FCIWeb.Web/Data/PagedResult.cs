using System;
using System.Collections.Generic;

namespace FCIWeb.Web.Data
{
    /// <summary>Pagina di risultati immutabile, con metadati sufficienti per navigazione UI.</summary>
    public sealed class PagedResult<T>
    {
        public PagedResult(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            PageNumber = Math.Max(1, pageNumber);
            PageSize = Math.Max(1, pageSize);
            TotalCount = Math.Max(0, totalCount);
        }

        public IReadOnlyList<T> Items { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1 && TotalPages > 0;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
