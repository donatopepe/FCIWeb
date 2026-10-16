using System;
using System.Globalization;

namespace FCIWeb.Core.Utility
{
    /// <summary>
    /// Conversione date per il dominio FCI.
    /// Porting cross-platform di <c>Source/Areas/FCI/Uility/DateUtil.cs</c>
    /// (rimossa la dipendenza inutilizzata da <c>System.Web</c>).
    /// </summary>
    public class DateUtil
    {
        /// <summary>Numero di anni oltre i quali una data e' considerata non valida.</summary>
        private const int MaxYears = 120;

        /// <summary>Formatta una data come <c>dd/MM/yyyy</c>; vuoto se null o troppo vecchia.</summary>
        public string DataToText(DateTime? value)
        {
            if (value == null) return String.Empty;
            DateTime localDate = DateTime.Now;
            if (value < localDate.AddYears(-MaxYears))
            {
                return String.Empty;
            }
            return ((DateTime)value).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converte una stringa in data provando i formati ddMMyy, ddMMyyyy, dd/MM/yyyy, dd/MM/yy
        /// e infine la cultura it-IT. Restituisce <c>null</c> se non valida o troppo vecchia.
        /// </summary>
        public DateTime? TextToDate(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            DateTime result;
            var dataOK = DateTime.TryParseExact(value, "ddMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
            if (!dataOK)
            {
                dataOK = DateTime.TryParseExact(value, "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
                if (!dataOK)
                {
                    dataOK = DateTime.TryParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
                    if (!dataOK)
                    {
                        dataOK = DateTime.TryParseExact(value, "dd/MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
                        if (!dataOK)
                        {
                            dataOK = DateTime.TryParse(value, CultureInfo.GetCultureInfo("it-IT"), DateTimeStyles.None, out result);
                            if (!dataOK)
                            {
                                return null;
                            }
                        }
                    }
                }
            }
            if (dataOK)
            {
                DateTime localDate = DateTime.Now;
                if (result < localDate.AddYears(-MaxYears))
                {
                    return null;
                }
            }
            return result;
        }
    }
}
