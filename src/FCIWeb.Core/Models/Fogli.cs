using System.Collections.Generic;

namespace FCIWeb.Core.Models
{
    /// <summary>
    /// Foglio (tabella configurabile) con stato, colonne e righe.
    /// Porting cross-platform di <c>Fogli</c>; le proprietà calcolate
    /// <c>Addetto</c>/<c>Struttura</c> sono in
    /// <see cref="FCIWeb.Core.Services.ResourcesDisplay"/>.
    /// </summary>
    public class Fogli : Comune
    {
        public Fogli()
        {
            Righe = new HashSet<Righe>();
            ProfiliWL = new HashSet<ProfiliWL>();
            LogStati = new HashSet<LogStati>();
            ProprietaColonne = new HashSet<Colonne>();
        }

        public string NomeLista { get; set; }
        public string NomeFile { get; set; }
        public string StileFoglio { get; set; }

        public ICollection<Colonne> ProprietaColonne { get; set; }
        public Stati Stato { get; set; }
        public ICollection<Righe> Righe { get; set; }
        public ICollection<ProfiliWL> ProfiliWL { get; set; }
        public ICollection<LogStati> LogStati { get; set; }

        public int? istID { get; set; }
        public string UOCode { get; set; }
        public string Matricola { get; set; }
    }

    /// <summary>Riga di un foglio. Porting di <c>Righe</c>.</summary>
    public class Righe : Comune
    {
        public int numRiga { get; set; }
        public string riga { get; set; } = "";
    }
}
