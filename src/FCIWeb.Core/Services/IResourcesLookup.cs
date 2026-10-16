namespace FCIWeb.Core.Services
{
    /// <summary>
    /// Accesso alle anagrafiche di supporto (utenti e unità organizzative).
    /// Nel legacy queste informazioni arrivavano da repository/web service proprietari;
    /// in cross-platform sono un'astrazione iniettabile (mock in DEMO_MODE, DB in produzione).
    /// </summary>
    public interface IResourcesLookup
    {
        /// <summary>Nome leggibile dell'utente a partire dalla matricola.</summary>
        string GetNomeUtente(string matricola);

        /// <summary>Descrizione dell'unità organizzativa a partire dal codice UO.</summary>
        string GetDescrizioneUO(string uocode);
    }
}
