using FCIWeb.Core.Models;
using Xunit;

namespace FCIWeb.Core.Tests
{
    public class ModelsTests
    {
        [Fact]
        public void ParametriApp_Defaults_AreActive()
        {
            Assert.True(new ParametriApp().IsActive);
        }

        [Fact]
        public void Stati_CollectionsAndFlags_Initialised()
        {
            var stati = new Stati();
            Assert.NotNull(stati.StatiPossibili);
            Assert.True(stati.IsActive);
            Assert.True(stati.Pubblica);
            Assert.True(stati.Modificabile);
            Assert.False(stati.Cancellabile);
            Assert.False(stati.ObbligoMotivazione);
        }

        [Fact]
        public void Colonne_Defaults()
        {
            var colonne = new Colonne();
            Assert.NotNull(colonne.column);
            Assert.Equal("text", colonne.column.type);
            Assert.False(colonne.column.readOnly);
        }

        [Fact]
        public void Fogli_CollectionsInitialised()
        {
            var foglio = new Fogli();
            Assert.NotNull(foglio.Righe);
            Assert.NotNull(foglio.ProfiliWL);
            Assert.NotNull(foglio.LogStati);
            Assert.NotNull(foglio.ProprietaColonne);
        }

        [Fact]
        public void Righe_DefaultRigaIsEmptyString()
        {
            Assert.Equal("", new Righe().riga);
        }

        [Fact]
        public void ProfiliWL_Defaults()
        {
            var p = new ProfiliWL();
            Assert.True(p.IsUocode);
            Assert.True(p.IsActive);
            Assert.False(p.Notificata);
        }

        [Fact]
        public void LogStati_ImplementsAuditInfo()
        {
            Assert.IsAssignableFrom<IAuditInfo>(new LogStati());
            Assert.IsAssignableFrom<IAuditInfo>(new Comune());
        }
    }
}
