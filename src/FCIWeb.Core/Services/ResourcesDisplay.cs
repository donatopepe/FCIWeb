using System;
using FCIWeb.Core.Models;

namespace FCIWeb.Core.Services
{
    /// <summary>
    /// Risoluzione dei nomi leggibili (utente/UO) per le entità di dominio.
    /// Sostituisce le proprietà calcolate del legacy che istanziavano direttamente
    /// <c>ResourcesRepository</c> (assembly proprietario) dentro i modelli.
    /// </summary>
    /// <remarks>
    /// Comportamento: se il lookup è <c>null</c> o restituisce vuoto, viene restituito il valore
    /// grezzo (matricola/UOCode), così l'oggetto resta utilizzabile anche senza anagrafica.
    /// </remarks>
    public static class ResourcesDisplay
    {
        public static string AddettoUltimaModifica(this IAuditInfo entity, IResourcesLookup lookup)
            => Resolve(entity?.MatricolaUltimaModifica, lookup, isUo: false);

        public static string StrutturaUltimaModifica(this IAuditInfo entity, IResourcesLookup lookup)
            => Resolve(entity?.UOCodeUltimaModifica, lookup, isUo: true);

        public static string Addetto(this Fogli foglio, IResourcesLookup lookup)
            => Resolve(foglio?.Matricola, lookup, isUo: false);

        public static string Struttura(this Fogli foglio, IResourcesLookup lookup)
            => Resolve(foglio?.UOCode, lookup, isUo: true);

        public static string Descrizione(this ProfiliWL profilo, IResourcesLookup lookup)
            => Resolve(profilo?.uocodematricola, lookup, isUo: profilo != null && profilo.IsUocode);

        private static string Resolve(string value, IResourcesLookup lookup, bool isUo)
        {
            if (String.IsNullOrWhiteSpace(value)) return value;
            if (lookup == null) return value;

            try
            {
                var resolved = isUo ? lookup.GetDescrizioneUO(value) : lookup.GetNomeUtente(value);
                return String.IsNullOrWhiteSpace(resolved) ? value : resolved;
            }
            catch (Exception)
            {
                // Nessuna anagrafica disponibile: meglio il valore grezzo che un'eccezione in view.
                return value;
            }
        }
    }
}
