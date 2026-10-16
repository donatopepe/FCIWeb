using System;
using FCIWeb.Core.Models;
using FCIWeb.Core.Services;
using Xunit;

namespace FCIWeb.Core.Tests
{
    public class ResourcesDisplayTests
    {
        private sealed class FakeLookup : IResourcesLookup
        {
            public string UtenteResult { get; set; }
            public string UoResult { get; set; }
            public bool Throw { get; set; }

            public string GetNomeUtente(string matricola)
            {
                if (Throw) throw new InvalidOperationException("anagrafica non disponibile");
                return UtenteResult;
            }

            public string GetDescrizioneUO(string uocode)
            {
                if (Throw) throw new InvalidOperationException("anagrafica non disponibile");
                return UoResult;
            }
        }

        private static readonly FakeLookup Lookup = new FakeLookup
        {
            UtenteResult = "Mario Rossi",
            UoResult = "Filiale Centrale"
        };

        [Fact]
        public void AddettoUltimaModifica_ResolvesFromLookup()
        {
            var e = new Comune { MatricolaUltimaModifica = "M001" };
            Assert.Equal("Mario Rossi", e.AddettoUltimaModifica(Lookup));
        }

        [Fact]
        public void StrutturaUltimaModifica_ResolvesFromLookup()
        {
            var e = new Comune { UOCodeUltimaModifica = "UO1" };
            Assert.Equal("Filiale Centrale", e.StrutturaUltimaModifica(Lookup));
        }

        [Fact]
        public void Resolve_BlankValue_ReturnsValueUnchanged()
        {
            var e = new Comune { MatricolaUltimaModifica = "" };
            Assert.Equal("", e.AddettoUltimaModifica(Lookup));
        }

        [Fact]
        public void Resolve_NullLookup_ReturnsRawValue()
        {
            var e = new Comune { MatricolaUltimaModifica = "M001" };
            Assert.Equal("M001", e.AddettoUltimaModifica(null));
        }

        [Fact]
        public void Resolve_EmptyLookupResult_FallsBackToRawValue()
        {
            var lookup = new FakeLookup { UtenteResult = null };
            var e = new Comune { MatricolaUltimaModifica = "M001" };
            Assert.Equal("M001", e.AddettoUltimaModifica(lookup));
        }

        [Fact]
        public void Resolve_LookupThrows_DoesNotPropagate()
        {
            var lookup = new FakeLookup { Throw = true };
            var e = new Comune { MatricolaUltimaModifica = "M001" };
            Assert.Equal("M001", e.AddettoUltimaModifica(lookup));
        }

        [Fact]
        public void Fogli_Addetto_And_Struttura_Resolve()
        {
            var f = new Fogli { Matricola = "M002", UOCode = "UO2" };
            Assert.Equal("Mario Rossi", f.Addetto(Lookup));
            Assert.Equal("Filiale Centrale", f.Struttura(Lookup));
        }

        [Fact]
        public void ProfiliWL_Descrizione_UsesUoWhenIsUocode()
        {
            var p = new ProfiliWL { uocodematricola = "UO1", IsUocode = true };
            Assert.Equal("Filiale Centrale", p.Descrizione(Lookup));

            p.IsUocode = false;
            Assert.Equal("Mario Rossi", p.Descrizione(Lookup));
        }

        [Fact]
        public void LogStati_SharedAuditExtensions_Work()
        {
            var log = new LogStati { MatricolaUltimaModifica = "M003", UOCodeUltimaModifica = "UO3" };
            Assert.Equal("Mario Rossi", log.AddettoUltimaModifica(Lookup));
            Assert.Equal("Filiale Centrale", log.StrutturaUltimaModifica(Lookup));
        }
    }
}
