using System;

namespace FCIWeb.Core.Models
{
    /// <summary>
    /// Campi di tracciamento ("chi/quando/da quale UO") condivisi dalle entità modificabili.
    /// Porting cross-platform della parte dati di <c>FCIWeb.Areas.FCI.Models.Comune</c>;
    /// le proprietà calcolate che dipendevano da repository proprietari sono state spostate in
    /// <see cref="FCIWeb.Core.Services.ResourcesDisplay"/>.
    /// </summary>
    public interface IAuditInfo
    {
        int? istIDUltimaModifica { get; set; }
        string UOCodeUltimaModifica { get; set; }
        string MatricolaUltimaModifica { get; set; }
        DateTime? UltimaModifica { get; set; }
    }

    /// <summary>
    /// Base per le entità con identificativo <see cref="Guid"/> e audit.
    /// I nomi delle proprietà sono mantenuti identici al legacy per non rompere la mappatura EF.
    /// </summary>
    public class Comune : IAuditInfo
    {
        public Guid Guid { get; set; }

        public int? istIDUltimaModifica { get; set; }
        public string UOCodeUltimaModifica { get; set; }
        public string MatricolaUltimaModifica { get; set; }

        public DateTime? DataCreazione { get; set; }
        public DateTime? UltimaModifica { get; set; }
    }
}
