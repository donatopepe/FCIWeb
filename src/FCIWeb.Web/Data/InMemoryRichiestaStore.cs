using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FCIWeb.Web.Models;
using StatoRichiesta = FCIWeb.Core.Models.StatoRichiesta;

namespace FCIWeb.Web.Data
{
    /// <summary>
    /// Store in memoria thread-safe con dati demo. Nessun database richiesto: rende l'app
    /// avviabile e usabile subito (DEMO_MODE).
    /// </summary>
    public sealed class InMemoryRichiestaStore : IRichiestaStore
    {
        private readonly ConcurrentDictionary<Guid, Richiesta> _items = new ConcurrentDictionary<Guid, Richiesta>();

        public InMemoryRichiestaStore()
        {
            Seed();
        }

        private void Seed()
        {
            var now = DateTime.UtcNow;
            var demo = new[]
            {
                new Richiesta { NDG = "1001", Intestazione = "Apertura conto demo", UOCode = "UO001", Matricola = "M001", Stato = StatoRichiesta.Bozza, DataCreazione = now.AddDays(-5), Note = "Richiesta di esempio." },
                new Richiesta { NDG = "1002", Intestazione = "Aggiornamento anagrafica", UOCode = "UO002", Matricola = "M002", Stato = StatoRichiesta.Inviata, DataCreazione = now.AddDays(-4), Note = "Richiesta di esempio." },
                new Richiesta { NDG = "1003", Intestazione = "Verifica documenti", UOCode = "UO001", Matricola = "M001", Stato = StatoRichiesta.InLavorazione, DataCreazione = now.AddDays(-3), Note = "Richiesta di esempio." },
                new Richiesta { NDG = "1004", Intestazione = "Chiusura pratica", UOCode = "UO003", Matricola = "M003", Stato = StatoRichiesta.Chiusa, DataCreazione = now.AddDays(-2), Note = "Richiesta di esempio." },
                new Richiesta { NDG = "1005", Intestazione = "Richiesta annullata", UOCode = "UO002", Matricola = "M002", Stato = StatoRichiesta.Annullata, DataCreazione = now.AddDays(-1), Note = "Richiesta di esempio." }
            };

            foreach (var r in demo)
            {
                r.Id = Guid.NewGuid();
                _items[r.Id] = r;
            }
        }

        public IReadOnlyList<Richiesta> GetAll(string query = null)
        {
            var all = _items.Values.OrderByDescending(r => r.DataCreazione).AsEnumerable();
            if (!String.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim();
                all = all.Where(r =>
                    (r.NDG != null && r.NDG.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (r.Intestazione != null && r.Intestazione.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (r.UOCode != null && r.UOCode.Contains(q, StringComparison.OrdinalIgnoreCase)));
            }
            return all.ToList();
        }

        public Richiesta GetById(Guid id) => _items.TryGetValue(id, out var r) ? r : null;

        public Richiesta Add(Richiesta richiesta)
        {
            ArgumentNullException.ThrowIfNull(richiesta);
            richiesta.Id = richiesta.Id == Guid.Empty ? Guid.NewGuid() : richiesta.Id;
            richiesta.DataCreazione = richiesta.DataCreazione == default ? DateTime.UtcNow : richiesta.DataCreazione;
            _items[richiesta.Id] = richiesta;
            return richiesta;
        }

        public bool TryChangeState(Guid id, StatoRichiesta nuovo, string motivazione, out string errorMessage, Guid? expectedVersion = null)
        {
            errorMessage = null;
            if (!_items.TryGetValue(id, out var r))
            {
                errorMessage = "Richiesta non trovata.";
                return false;
            }

            if (expectedVersion.HasValue && expectedVersion.Value != r.Version)
            {
                errorMessage = "La richiesta è stata modificata da un'altra operazione. Ricaricare i dati e riprovare.";
                return false;
            }
            if (!r.PuoCambiareIn(nuovo))
            {
                errorMessage = $"Transizione non ammessa: {r.Stato} -> {nuovo}.";
                return false;
            }

            r.Stato = nuovo;
            r.Version = Guid.NewGuid();
            if (!String.IsNullOrWhiteSpace(motivazione))
            {
                r.Note = String.IsNullOrWhiteSpace(r.Note) ? motivazione : r.Note + " | " + motivazione;
            }
            return true;
        }

        public bool Delete(Guid id) => _items.TryRemove(id, out _);
    }
}
