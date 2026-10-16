using System.Collections.Generic;

namespace FCIWeb.Core.Models
{
    /// <summary>Profilo di abilitazione. Porting di <c>Profili</c>.</summary>
    public class Profili
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descrizione { get; set; }
        public bool IsActive { get; set; } = true;
        public bool Compilazione { get; set; } = false;
        public bool ModificaLista { get; set; } = false;
        public bool Esportazione { get; set; } = false;
        public bool SoloPubbliche { get; set; } = true;
        public bool TutteVisibili { get; set; } = false;
    }

    /// <summary>Associazione profilo &lt;-&gt; utente/UO. Porting di <c>ProfiliWL</c>.</summary>
    public class ProfiliWL
    {
        public long Id { get; set; }
        public string uocodematricola { get; set; }
        public bool IsUocode { get; set; } = true;
        public Profili profilo { get; set; }
        public bool IsActive { get; set; } = true;
        public int? istID { get; set; }
        public bool Notificata { get; set; } = false;

        /// <summary>
        /// Descrizione leggibile di <see cref="uocodematricola"/> (utente o UO).
        /// Nel legacy era una proprietà calcolata dipendente da un repository proprietario:
        /// qui richiede un <see cref="FCIWeb.Core.Services.IResourcesLookup"/>.
        /// </summary>
        public string Descrizione(FCIWeb.Core.Services.IResourcesLookup lookup)
            => FCIWeb.Core.Services.ResourcesDisplay.Descrizione(this, lookup);
    }
}
