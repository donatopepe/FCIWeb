using System;

namespace FCIWeb.Core.Models
{
    /// <summary>Definizione di colonna per la griglia (serializzata in <see cref="Colonne"/>).</summary>
    public class column
    {
        public string title { get; set; }
        public string type { get; set; } = "text";
        public string source { get; set; }
        public string width { get; set; }
        public bool readOnly { get; set; } = false;
    }

    /// <summary>Metadati di una colonna di un foglio. Porting di <c>Colonne</c>.</summary>
    public class Colonne
    {
        public Guid Guid { get; set; }
        public int numColonna { get; set; }
        public column column { get; set; } = new column();
    }
}
