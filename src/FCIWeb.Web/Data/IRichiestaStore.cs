using System;
using System.Collections.Generic;
using FCIWeb.Web.Models;
using StatoRichiesta = FCIWeb.Core.Models.StatoRichiesta;

namespace FCIWeb.Web.Data
{
    /// <summary>Accesso ai dati delle richieste (memoria in DEMO_MODE, EF Core in M3).</summary>
    public interface IRichiestaStore
    {
        IReadOnlyList<Richiesta> GetAll(string query = null);

        PagedResult<Richiesta> GetPage(string query, int pageNumber, int pageSize);

        Richiesta GetById(Guid id);

        Richiesta Add(Richiesta richiesta);

        /// <summary>Cambia stato rispettando le transizioni ammesse.</summary>
        bool TryChangeState(Guid id, StatoRichiesta nuovo, string motivazione, out string errorMessage, Guid? expectedVersion = null);

        bool Delete(Guid id);
    }
}
