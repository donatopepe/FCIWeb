using System;
using System.Collections.Generic;
using System.Linq;
using FCIWeb.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FCIWeb.Data
{
    /// <summary>Repository EF Core con persistenza e transizioni atomiche e validate.</summary>
    public sealed class EfRichiestaRepository : IRichiestaRepository
    {
        private readonly FCIWebContext _db;

        public EfRichiestaRepository(FCIWebContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public IReadOnlyList<Richiesta> GetAll(string query = null)
        {
            IQueryable<Richiesta> richieste = _db.Richieste.AsNoTracking();
            if (!String.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim();
                richieste = richieste.Where(r => r.NDG.Contains(q) || r.Intestazione.Contains(q) ||
                                                 (r.UOCode != null && r.UOCode.Contains(q)));
            }
            return richieste.OrderByDescending(r => r.DataCreazione).ToList();
        }

        public (IReadOnlyList<Richiesta> Items, int TotalCount) GetPage(string query, int pageNumber, int pageSize)
        {
            var safePageSize = Math.Clamp(pageSize, 1, 50);
            var safePage = Math.Max(1, pageNumber);
            IQueryable<Richiesta> filtered = _db.Richieste.AsNoTracking();
            if (!String.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim();
                filtered = filtered.Where(r => r.NDG.Contains(q) || r.Intestazione.Contains(q) ||
                                               (r.UOCode != null && r.UOCode.Contains(q)));
            }

            var total = filtered.Count();
            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)safePageSize);
            if (totalPages > 0) safePage = Math.Min(safePage, totalPages);
            var items = filtered.OrderByDescending(r => r.DataCreazione).ThenBy(r => r.Id)
                .Skip((safePage - 1) * safePageSize).Take(safePageSize).ToList();
            return (items, total);
        }

        public Richiesta GetById(Guid id) => _db.Richieste.AsNoTracking().SingleOrDefault(r => r.Id == id);

        public Richiesta Add(Richiesta richiesta)
        {
            ArgumentNullException.ThrowIfNull(richiesta);
            if (richiesta.Id == Guid.Empty) richiesta.Id = Guid.NewGuid();
            if (richiesta.Version == Guid.Empty) richiesta.Version = Guid.NewGuid();
            if (richiesta.DataCreazione == default) richiesta.DataCreazione = DateTime.UtcNow;
            if (String.IsNullOrWhiteSpace(richiesta.NDG)) throw new ArgumentException("NDG obbligatorio.", nameof(richiesta));
            if (String.IsNullOrWhiteSpace(richiesta.Intestazione)) throw new ArgumentException("Intestazione obbligatoria.", nameof(richiesta));

            _db.Richieste.Add(richiesta);
            _db.SaveChanges();
            return richiesta;
        }

        public bool TryChangeState(Guid id, StatoRichiesta nuovo, string motivazione, out string errorMessage, Guid? expectedVersion = null)
        {
            errorMessage = null;
            var richiesta = _db.Richieste.AsNoTracking().SingleOrDefault(r => r.Id == id);
            if (richiesta == null)
            {
                errorMessage = "Richiesta non trovata.";
                return false;
            }
            if (!richiesta.PuoCambiareIn(nuovo))
            {
                errorMessage = $"Transizione non ammessa: {richiesta.Stato} -> {nuovo}.";
                return false;
            }

            var oldVersion = expectedVersion ?? richiesta.Version;
            var newVersion = Guid.NewGuid();
            var newNote = String.IsNullOrWhiteSpace(motivazione)
                ? richiesta.Note
                : String.IsNullOrWhiteSpace(richiesta.Note) ? motivazione : richiesta.Note + " | " + motivazione;

            // Atomic compare-and-swap: state validation and version predicate prevent lost updates
            // even when another request changes the row after our read.
            var updated = _db.Richieste
                .Where(r => r.Id == id && r.Version == oldVersion)
                .ExecuteUpdate(setters => setters
                    .SetProperty(r => r.Stato, nuovo)
                    .SetProperty(r => r.Version, newVersion)
                    .SetProperty(r => r.Note, newNote));

            if (updated == 0)
            {
                errorMessage = "La richiesta è stata modificata da un'altra operazione. Ricaricare i dati e riprovare.";
                return false;
            }
            return true;
        }

        public bool Delete(Guid id)
        {
            var richiesta = _db.Richieste.SingleOrDefault(r => r.Id == id);
            if (richiesta == null) return false;
            _db.Richieste.Remove(richiesta);
            _db.SaveChanges();
            return true;
        }
    }
}
