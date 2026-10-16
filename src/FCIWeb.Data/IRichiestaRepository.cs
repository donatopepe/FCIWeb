using System;
using System.Collections.Generic;
using FCIWeb.Core.Models;

namespace FCIWeb.Data
{
    /// <summary>Operazioni di persistenza delle richieste (stessa astrazione usata dalla Web UI).</summary>
    public interface IRichiestaRepository
    {
        IReadOnlyList<Richiesta> GetAll(string query = null);
        Richiesta GetById(Guid id);
        Richiesta Add(Richiesta richiesta);
        bool TryChangeState(Guid id, StatoRichiesta nuovo, string motivazione, out string errorMessage);
        bool Delete(Guid id);
    }
}
