using System.Collections.Generic;

namespace FCIWeb.Core.Models
{
    /// <summary>Transizione di stato ammessa. Porting di <c>StatiPossibili</c>.</summary>
    public class StatiPossibili
    {
        public int Id { get; set; }
        public int Stato { get; set; }
    }

    /// <summary>Stato del flusso di una richiesta/foglio. Porting di <c>Stati</c>.</summary>
    public class Stati
    {
        public Stati()
        {
            StatiPossibili = new HashSet<StatiPossibili>();
        }

        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descrizione { get; set; }
        public bool IsActive { get; set; } = true;
        public bool Pubblica { get; set; } = true;
        public bool Modificabile { get; set; } = true;
        public bool Cancellabile { get; set; } = false;
        public bool ObbligoMotivazione { get; set; } = false;
        public ICollection<StatiPossibili> StatiPossibili { get; set; }
    }

    /// <summary>Voce di log dei cambi di stato. Porting di <c>LogStati</c> (parte dati).</summary>
    public class LogStati : IAuditInfo
    {
        public long Id { get; set; }
        public Stati stato { get; set; }
        public string Motivazione { get; set; }

        public int? istIDUltimaModifica { get; set; }
        public string UOCodeUltimaModifica { get; set; }
        public string MatricolaUltimaModifica { get; set; }
        public System.DateTime? UltimaModifica { get; set; }
    }
}
