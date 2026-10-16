using System;
using System.Linq;
using FCIWeb.Core.Models;
using FCIWeb.Data;
using Xunit;

namespace FCIWeb.Data.Tests
{
    [Collection("SQLite")]
    public sealed class EfRichiestaRepositoryTests : IAsyncLifetime
    {
        private readonly SqliteFixture _fixture;
        public EfRichiestaRepositoryTests(SqliteFixture fixture) => _fixture = fixture;

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            using var db = _fixture.CreateContext();
            db.Richieste.RemoveRange(db.Richieste);
            await db.SaveChangesAsync();
        }

        public System.Threading.Tasks.Task DisposeAsync() => System.Threading.Tasks.Task.CompletedTask;

        private static Richiesta New(string ndg, string name, StatoRichiesta state = StatoRichiesta.Bozza) => new Richiesta
        {
            Id = Guid.NewGuid(), NDG = ndg, Intestazione = name, UOCode = "UO-01",
            Matricola = "M-01", DataCreazione = DateTime.UtcNow, Stato = state
        };

        [Fact]
        public void AddAndRead_PersistsAfterContextIsReopened()
        {
            var entity = New("A-100", "Persistent request");
            using (var db = _fixture.CreateContext()) new EfRichiestaRepository(db).Add(entity);

            using var reopened = _fixture.CreateContext();
            var result = new EfRichiestaRepository(reopened).GetById(entity.Id);
            Assert.NotNull(result);
            Assert.Equal("A-100", result.NDG);
            Assert.Equal("Persistent request", result.Intestazione);
        }

        [Fact]
        public void GetAll_FiltersAndOrdersByCreationDate()
        {
            var first = New("A-101", "First"); first.DataCreazione = DateTime.UtcNow.AddDays(-1);
            var second = New("B-202", "Second"); second.DataCreazione = DateTime.UtcNow;
            using var db = _fixture.CreateContext();
            var repo = new EfRichiestaRepository(db);
            repo.Add(first);
            repo.Add(second);
            Assert.Equal(second.Id, repo.GetAll()[0].Id);
            Assert.Single(repo.GetAll("B-20"));
            Assert.Single(repo.GetAll("Second"));
            Assert.Equal(2, repo.GetAll("UO-01").Count);
        }

        [Fact]
        public void ChangeState_ValidTransitionPersistsStateAndMotivation()
        {
            var entity = New("A-103", "Transition", StatoRichiesta.Bozza);
            using (var db = _fixture.CreateContext()) new EfRichiestaRepository(db).Add(entity);
            using (var db = _fixture.CreateContext())
            {
                var repo = new EfRichiestaRepository(db);
                Assert.True(repo.TryChangeState(entity.Id, StatoRichiesta.Inviata, "approvata", out var error));
                Assert.Null(error);
            }
            using var read = _fixture.CreateContext();
            var updated = new EfRichiestaRepository(read).GetById(entity.Id);
            Assert.Equal(StatoRichiesta.Inviata, updated.Stato);
            Assert.Contains("approvata", updated.Note);
        }

        [Fact]
        public void ChangeState_InvalidTransitionDoesNotMutate()
        {
            var entity = New("A-104", "Invalid transition", StatoRichiesta.Bozza);
            using (var db = _fixture.CreateContext()) new EfRichiestaRepository(db).Add(entity);
            using (var db = _fixture.CreateContext())
            {
                var repo = new EfRichiestaRepository(db);
                Assert.False(repo.TryChangeState(entity.Id, StatoRichiesta.Chiusa, null, out var error));
                Assert.NotNull(error);
            }
            using var read = _fixture.CreateContext();
            Assert.Equal(StatoRichiesta.Bozza, new EfRichiestaRepository(read).GetById(entity.Id).Stato);
        }

        [Fact]
        public void ChangeState_StaleContextReturnsConcurrencyConflict()
        {
            var entity = New("A-106", "Concurrent change", StatoRichiesta.Bozza);
            using (var seed = _fixture.CreateContext()) new EfRichiestaRepository(seed).Add(entity);

            using var contextA = _fixture.CreateContext();
            using var contextB = _fixture.CreateContext();
            var repoA = new EfRichiestaRepository(contextA);
            var repoB = new EfRichiestaRepository(contextB);
            var staleVersion = repoB.GetById(entity.Id).Version;

            Assert.True(repoA.TryChangeState(entity.Id, StatoRichiesta.Inviata, "first", out var firstError));
            Assert.Null(firstError);
            Assert.False(repoB.TryChangeState(entity.Id, StatoRichiesta.Annullata, "stale", out var conflict, staleVersion));
            Assert.Contains("modificata da un'altra operazione", conflict);

            using var read = _fixture.CreateContext();
            var unchanged = new EfRichiestaRepository(read).GetById(entity.Id);
            Assert.Equal(StatoRichiesta.Inviata, unchanged.Stato);
            Assert.Contains("first", unchanged.Note);
            Assert.DoesNotContain("stale", unchanged.Note ?? "");
        }

        [Fact]
        public void ChangeState_UnknownIdReturnsFalse()
        {
            using var db = _fixture.CreateContext();
            Assert.False(new EfRichiestaRepository(db).TryChangeState(Guid.NewGuid(), StatoRichiesta.Inviata, null, out var error));
            Assert.Contains("non trovata", error);
        }

        [Fact]
        public void Delete_RemovesEntity()
        {
            var entity = New("A-105", "To delete");
            using (var db = _fixture.CreateContext()) new EfRichiestaRepository(db).Add(entity);
            using (var db = _fixture.CreateContext()) Assert.True(new EfRichiestaRepository(db).Delete(entity.Id));
            using var read = _fixture.CreateContext();
            Assert.Null(new EfRichiestaRepository(read).GetById(entity.Id));
        }

        [Fact]
        public void Add_RejectsMissingRequiredFields()
        {
            using var db = _fixture.CreateContext();
            Assert.Throws<ArgumentException>(() => new EfRichiestaRepository(db).Add(New("", "missing NDG")));
        }
    }
}
