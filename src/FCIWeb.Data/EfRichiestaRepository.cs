using System;
using System.Collections.Generic;
using System.Linq;
using FCIWeb.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FCIWeb.Data
{
    /// <summary>Repository EF Core con persistenza e transizioni di stato validate.</summary>
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

        public Richiesta GetById(Guid id) => _db.Richieste.AsNoTracking().SingleOrDefault(r => r.Id == id);

        public Richiesta Add(Richiesta richiesta)
        {
            ArgumentNullException.ThrowIfNull(richiesta);
            if (richiesta.Id == Guid.Empty) richiesta.Id = Guid.NewGuid();
            if (richiesta.DataCreazione == default) richiesta.DataCreazione = DateTime.UtcNow;
            if (String.IsNullOrWhiteSpace(richiesta.NDG)) throw new ArgumentException("NDG obbligatorio.", nameof(richiesta));
            if (String.IsNullOrWhiteSpace(richiesta.Intestazione)) throw new ArgumentException("Intestazione obbligatoria.", nameof(richiesta));

            _db.Richieste.Add(richiesta);
            _db.SaveChanges();
            return richiesta;
        }

        public bool TryChangeState(Guid id, StatoRichiesta nuovo, string motivazione, out string errorMessage)
        {
            errorMessage = null;
            var richiesta = _db.Richieste.SingleOrDefault(r => r.Id == id);
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

            richiesta.Stato = nuovo;
            if (!String.IsNullOrWhiteSpace(motivazione))
                richiesta.Note = String.IsNullOrWhiteSpace(richiesta.Note) ? motivazione : richiesta.Note + " | " + motivazione;

            _db.SaveChanges();
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
