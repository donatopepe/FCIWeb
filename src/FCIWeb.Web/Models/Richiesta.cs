using FCIWeb.Core.Models;

namespace FCIWeb.Web.Models
{
    /// <summary>
    /// Alias di compatibilità MVC: il modello autorevole è ora nel dominio condiviso,
    /// così Core, EF Core e Web usano la stessa entità.
    /// </summary>
    public class Richiesta : FCIWeb.Core.Models.Richiesta
    {
    }
}
