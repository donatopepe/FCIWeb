# TODO — Follow-up porting M3/M5–M8

## Obiettivo corrente
M3 migrations SQLite realizzate e testate; proseguire verifiche provider/porting use-case senza
attribuire esito SQL Server non osservato.

## Metrica + soglia del ciclo migration
Tasso casi migration superati = passati/totali; soglia **100%** sui test SQLite; build
`/warnaserror` a zero warning/errori.

## Casi verificati
1. Database SQLite vuoto: `Migrate()` crea schema/table.
2. Secondo `Migrate()` idempotente.
3. Insert dopo migration e lettura dopo nuovo contesto; record preservato.
4. Repository CRUD/transizioni: coperti da test SQLite separati.

## M3 completato
- [x] Tool `dotnet-ef` pinned a 8.0.11 in `.config/dotnet-tools.json`.
- [x] `IDesignTimeDbContextFactory<FCIWebContext>`: SQLite default; SQL Server soltanto se provider e connection env sono configurati.
- [x] Migration iniziale `InitialCreate` + snapshot.
- [x] Bootstrap runtime `Database.Migrate()`.
- [x] Adottare database pregresso `EnsureCreated` senza perdita record: verifica colonne e registra migration iniziale; test dedicato.
- [x] Fixture SQLite usa migrazioni; test migrazione idempotente/data retention.
- [x] Build Docker include progetto Data.
- [x] Suite Linux: Core 96, Data 11, Web 10 = 117 test.
- [x] Build Release `/warnaserror`: zero warning/errori (con restore precedentemente eseguito).

## M5 partial — concorrenza applicativa completata su SQLite
- [x] Version token GUID application-managed in entity e migration.
- [x] Compare-and-swap transizione su ID + expected version.
- [x] Test two-context stale write rifiutato e dati aggiornati preservati.
- [ ] Ripetere test su SQL Server reale/disposable autorizzato.

## Prossimi task aperti
- [ ] SQL Server provider: eseguire migration SQL Server con database temporaneo autorizzato; non dichiarare pronto prima.
- [ ] M6: completare viste legacy.
- [ ] M7: porting documenti OpenXML.
- [ ] M8: multi-arch e release.

## Gate
- Segreti working tree/history puliti.
- Nessun push senza consenso; nessuna modifica distruttiva a DB persistente.
- Non dichiarare SQL Server runtime testato senza log di istanza reale disposable.
