# TODO — test e porting cross-platform

## Obiettivo
Portare il livello dati M3 a EF Core con SQLite demo e SQL Server production; validare l'app come
eseguibile e persistente su Linux.

## Metrica + soglia
**Metrica**: tasso test superati = test passati / test eseguiti. **Soglia: 100%** per suite intera,
più build `/warnaserror` a zero warning/errori e Docker smoke test reale.

## Casi definiti prima dell'implementazione (ordine, evidenza)
1. Add/read richiesta SQLite; leggibilità dopo dispose e riapertura `DbContext` (stesso SQLite connection).
2. Filtri NDG/intestazione/UO e ordine per data.
3. Transizione valida persiste stato + motivazione; transizione invalida non muta DB.
4. Identificativo inesistente gestito; delete rimuove record.
5. Web app usa SQLite persistente per default, `FCIWEB_STORE=memory` seleziona seed volatile.
6. `docker compose up --build` avvia login, crea DB su volume, HTTP login 200.

## Dipendenze
- .NET 8, EF Core 8.0.11, SQLite, SQL Server provider.
- Abilitato CodeGraph; index iniziale: 446 file, 6509 nodi, 25918 archi.

## Avanzamento
- [x] Core domain `Richiesta` + stati condivisi.
- [x] `src/FCIWeb.Data`: `FCIWebContext`, mapping, indici, repository EF.
- [x] SQLite fixture + 7 test di persistenza.
- [x] Wiring web: SQLite persistente default; `FCIWEB_STORE=memory` per store demo volatile.
- [x] Dockerfile include Data, directory DB owned by app; volume persistente in Compose.
- [x] Suite completa su Docker SDK Linux, inclusa creazione e lettura HTTP del database SQLite: 113/113.
- [x] Build Release `/warnaserror`: 0 warnings/errors.
- [x] Docker image build e Compose up: login endpoint 200, DB creato nel volume.
- [x] Web integration test usa SQLite temp file: crea via POST autenticato e trova richiesta via GET.
- [ ] Migrazione schema/versioning (attualmente `EnsureCreated`, non migrazioni EF).
- [ ] Test SQL Server integration (serve un'istanza SQL Server disponibile e disposable).

## Gate
`bash scripts/check-secrets.sh`; `bash scripts/check-history-secrets.sh`; `dotnet test`; build warn-as-error;
no proprietary binaries; no push. Non dichiarare migrations/SQL production testati finché i relativi test non girano.
