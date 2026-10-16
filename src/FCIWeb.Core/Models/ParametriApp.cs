using System;

namespace FCIWeb.Core.Models
{
    /// <summary>
    /// Parametro applicativo configurabile (chiave/valore).
    /// Porting cross-platform di <c>FCIWeb.Areas.FCI.Models.ParametriApp</c>.
    /// </summary>
    public class ParametriApp
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Val { get; set; }
        public string Descrizione { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
