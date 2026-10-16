using System;
using System.Collections.Generic;
using FCIWeb.Core.Models;
using FCIWeb.Data;

namespace FCIWeb.Web.Data
{
    /// <summary>Adapta repository EF Core all'interfaccia attesa dai controller MVC.</summary>
    public sealed class EfRichiestaStore : IRichiestaStore
    {
        private readonly IRichiestaRepository _repository;

        public EfRichiestaStore(IRichiestaRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public IReadOnlyList<FCIWeb.Web.Models.Richiesta> GetAll(string query = null)
        {
            var result = new List<FCIWeb.Web.Models.Richiesta>();
            foreach (var r in _repository.GetAll(query)) result.Add(ToWeb(r));
            return result;
        }

        public FCIWeb.Web.Models.Richiesta GetById(Guid id)
        {
            var result = _repository.GetById(id);
            return result == null ? null : ToWeb(result);
        }

        public FCIWeb.Web.Models.Richiesta Add(FCIWeb.Web.Models.Richiesta richiesta)
        {
            // Persist an instance of the mapped Core entity, not the MVC compatibility subtype.
            var added = _repository.Add(ToCore(richiesta));
            return ToWeb(added);
        }

        private static FCIWeb.Core.Models.Richiesta ToCore(FCIWeb.Core.Models.Richiesta r)
            => new FCIWeb.Core.Models.Richiesta
            {
                Id = r.Id,
                NDG = r.NDG,
                Intestazione = r.Intestazione,
                UOCode = r.UOCode,
                Matricola = r.Matricola,
                DataCreazione = r.DataCreazione,
                Stato = r.Stato,
                Note = r.Note,
                Version = r.Version
            };

        public bool TryChangeState(Guid id, StatoRichiesta nuovo, string motivazione, out string errorMessage, Guid? expectedVersion = null)
            => _repository.TryChangeState(id, nuovo, motivazione, out errorMessage, expectedVersion);

        public bool Delete(Guid id) => _repository.Delete(id);

        private static FCIWeb.Web.Models.Richiesta ToWeb(FCIWeb.Core.Models.Richiesta r)
            => new FCIWeb.Web.Models.Richiesta
            {
                Id = r.Id,
                NDG = r.NDG,
                Intestazione = r.Intestazione,
                UOCode = r.UOCode,
                Matricola = r.Matricola,
                DataCreazione = r.DataCreazione,
                Stato = r.Stato,
                Note = r.Note,
                Version = r.Version
            };
    }
}
