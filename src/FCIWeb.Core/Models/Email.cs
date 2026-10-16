using System;

namespace FCIWeb.Core.Models
{
    /// <summary>Testo email configurabile. Porting di <c>EmailTesti</c>.</summary>
    public class EmailTesti
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Oggetto { get; set; }
        public string Corpo { get; set; }
        public bool Html { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Configurazione SMTP. Porting di <c>EmailSettings</c>.</summary>
    /// <remarks>
    /// Contiene credenziali SMTP: nel porting cross-platform questi campi vanno valorizzati da
    /// configurazione/secret store e **non** committati (vedi SECURITY.md).
    /// </remarks>
    public class EmailSettings
    {
        public int id { get; set; }
        public string Name { get; set; }
        public bool isActive { get; set; }
        public string SmtpServer { get; set; }
        public int? SmtpPort { get; set; }
        public bool? SmtpSSL { get; set; }
        public string SenderName { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public bool isSimulated { get; set; }
        public string SimulatedEmail { get; set; }
    }

    /// <summary>Esito di un invio email. Porting di <c>EmailTipo</c>.</summary>
    public class EmailTipo
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool IsBodyHtml { get; set; } = true;
        public bool Result { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>Log di invio email. Porting di <c>LogEmail</c>.</summary>
    public class LogEmail : EmailTipo
    {
        public Guid Guid { get; set; }
        public DateTime Data { get; set; }
        public string Note { get; set; }
    }
}
