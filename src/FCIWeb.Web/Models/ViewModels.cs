using System.ComponentModel.DataAnnotations;

namespace FCIWeb.Web.Models
{
    /// <summary>Modello di login (validazione + anti overposting).</summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username obbligatorio.")]
        [StringLength(64)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password obbligatoria.")]
        [StringLength(128)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    /// <summary>Modello di creazione richiesta: espone solo i campi consentiti (no overposting).</summary>
    public class RichiestaCreateModel
    {
        [Required(ErrorMessage = "NDG obbligatorio.")]
        [StringLength(32)]
        public string NDG { get; set; }

        [Required(ErrorMessage = "Intestazione obbligatoria.")]
        [StringLength(200)]
        public string Intestazione { get; set; }

        [StringLength(32)]
        public string UOCode { get; set; }

        [StringLength(32)]
        public string Matricola { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }
    }
}
