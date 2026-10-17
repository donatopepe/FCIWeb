using System;
using System.Collections.Generic;
using FCIWeb.Core.Models;

namespace FCIWeb.Data
{
    /// <summary>Operazioni di persistenza delle richieste (stessa astrazione usata dalla Web UI).</summary>
    public interface IRichiestaRepository
    {
        IReadOnlyList<Richiesta> GetAll(string query = null);
        (IReadOnlyList<Richiesta> Items, int TotalCount) GetPage(string query, int pageNumber, int pageSize);
        Richiesta GetById(Guid id);
        Richiesta Add(Richiesta richiesta);
        bool TryChangeState(Guid id, StatoRichiesta nuovo, string motivazione, out string errorMessage, Guid? expectedVersion = null);
        bool Delete(Guid id);
    }
}
