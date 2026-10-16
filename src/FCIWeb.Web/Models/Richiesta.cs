using System;
using System.Collections.Generic;
using System.Linq;

namespace FCIWeb.Web.Models
{
    /// <summary>Stati del flusso di una richiesta (demo).</summary>
    public enum StatoRichiesta
    {
        Bozza = 0,
        Inviata = 1,
        InLavorazione = 2,
        Chiusa = 3,
        Annullata = 4
    }

    /// <summary>
    /// Richiesta di lavorazione. Modello applicativo della web app demo;
    /// in M3 verrà persistito su EF Core (SQLite/SQL Server).
    /// </summary>
    public class Richiesta
    {
        public Guid Id { get; set; }
        public string NDG { get; set; }
        public string Intestazione { get; set; }
        public string UOCode { get; set; }
        public string Matricola { get; set; }
        public DateTime DataCreazione { get; set; }
        public StatoRichiesta Stato { get; set; }
        public string Note { get; set; }

        /// <summary>Transizioni di stato ammesse dal flusso.</summary>
        public static IReadOnlyList<StatoRichiesta> TransizioniAmmesse(StatoRichiesta stato)
        {
            switch (stato)
            {
                case StatoRichiesta.Bozza:
                    return new[] { StatoRichiesta.Inviata, StatoRichiesta.Annullata };
                case StatoRichiesta.Inviata:
                    return new[] { StatoRichiesta.InLavorazione, StatoRichiesta.Annullata };
                case StatoRichiesta.InLavorazione:
                    return new[] { StatoRichiesta.Chiusa, StatoRichiesta.Annullata };
                default:
                    return Array.Empty<StatoRichiesta>();
            }
        }

        public static bool IsTerminale(StatoRichiesta stato)
            => stato == StatoRichiesta.Chiusa || stato == StatoRichiesta.Annullata;

        public bool PuoCambiareIn(StatoRichiesta nuovo)
            => TransizioniAmmesse(Stato).Contains(nuovo);
    }
}
