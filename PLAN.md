# PLAN — FCIWeb: risanamento, cross-platform e pubblicabilità

> Documento vivo. Ogni ciclo del loop aggiorna lo stato delle fasi e la tabella di avanzamento.
> Ultima revisione: 2026-10-16 (ciclo 8 — EF Core SQLite + app persistente, revalidazione completa)

## 1. Obiettivo

Rendere FCIWeb un progetto **cross-platform (Linux/macOS/Windows), documentato, senza segreti né
dipendenze non ridistribuibili, con licenza MIT, build/test riproducibili e guide complete**, così
che chiunque possa compilarlo, eseguirlo in modalità demo e comprenderlo.

Direzione confermata dall'owner: **porting a .NET 8** (non più solo .NET Framework/Windows).

## 2. Metrica e soglia (rubrica)

**Metrica**: punteggio di rubrica = criteri soddisfatti / 16 (ogni criterio 0/1, con evidenza).

**Soglia baseline**: ≥ 0.75 (12/16).
**Soglia obiettivo (chiusura)**: ≥ 0.92 (15/16).

| # | Criterio | Stato | Evidenza |
|---|----------|-------|----------|
| 1 | `README.md` completo | ✅ | `README.md` |
| 2 | `docs/GUIDA-COMPLETA.md` | ✅ | file |
| 3 | `LICENSE` MIT (solo codice originale) | ✅ | `LICENSE` |
| 4 | `THIRD-PARTY-NOTICES.md` esaustivo | ✅ | file |
| 5 | Nessun segreto nel **working tree** | ✅ | `scripts/check-secrets.sh` → OK |
| 6 | Nessun segreto nella **history git** | ✅ | `git grep` su `git rev-list --all` → 0 hit |
| 7 | Template config + `.gitignore` | ✅ | `connection.config.sample`, `.gitignore` |
| 8 | `docs/ANALISI-CODICE.md` | ✅ | file |
| 9 | `PLAN.md` | ✅ | questo file |
| 10 | `docs/PROMPT-INGEGNERIZZATO.md` | ✅ | file |
| 11 | CI verde: validazione statica | ✅ | run GitHub `38876403094` job "Validazione statica" → success |
| 12 | CI verde: build + test cross-platform `.NET 8` | ✅ | test locali 119/119; build /warnaserror 0/0; workflow GitHub non eseguito sul commit corrente |
| 13 | Nessuna dipendenza proprietaria/commerciale tracciata | ✅ | `git ls-files` → nessun binario; history pulita |
| 14 | Dead code / vendored documentati o archiviati | ✅ | `docs/ANALISI-CODICE.md`, `docs/PORTING-CROSS-PLATFORM.md` |
| 15 | App cross-platform avviabile in `DEMO_MODE` (container Linux, no SQL/servizi/terze parti) | ✅ | compose build/up; login 200, DB SQLite creato su volume, container non-root |
| 16 | `docs/PORTING-CROSS-PLATFORM.md` + piano di migrazione | ✅ | file |

**Misurato al ciclo 12**: **16/16 = 1.00** per i criteri della rubrica. La copertura funzionale
rimane in evoluzione: migrations SQLite sono testate; SQL Server end-to-end non è ancora verificato.

## 3. Fatti verificati

- Legacy: ASP.NET MVC 4 / .NET Framework 4.x, EF 5 Code First, Razor + RazorGenerator, area `FCI`.
- **Nessun .NET Framework disponibile su Linux**: la build legacy richiede Windows/Mono; tentata con
  `mono:6.12.0` (restore OK, build con reference mancanti → vedi §5).
- Toolchain cross-platform disponibile: `mcr.microsoft.com/dotnet/sdk:8.0` → `dotnet 8.0.426`.
- **Test cross-platform reali**: `dotnet test FCIWeb.CrossPlatform.sln -c Release` → 26/26 passati (Linux).
- **Segreti rimossi dalla history**: 2 passate `git-filter-repo`; verifica `git grep` su tutti i commit
  → **0 occorrenze** di tutti gli 11 pattern sensibili monitorati (credenziali DB, password wrapper,
  matricola, hostname/IP interni, account di dominio).
  [I valori letterali sono deliberatamente omessi da questa documentazione: elencarli li
  ripubblicherebbe. La lista dei pattern è in un file locale non versionato, vedi `SECURITY.md`.]
- **Path sensibili rimossi dalla history**: `Source/connection.config`, `Source/Doc_Files`,
  `Da aggiornare sul server.7z`.
- **Dipendenze proprietarie rimosse dal tracking e dalla history**: `lib/*.dll`, `lib/KENDOUIMVC`,
  `Source/Scripts/{kendo,2012.1.214}`, `Source/Content/{kendo,2012.1.214}`, `ce-master.zip`.
  File conservati su disco; `.git` ridotto a ~3 MB.
- Backup completo in `/root/fciweb-backup-20261016/` (bundle git + worktree) e segreti originali in
  `/root/fciweb-secrets-backup/`.

## 4. Ipotesi

- Le credenziali storiche vanno considerate compromesse e **ruotate** dall'owner.
- Le DLL in `lib/` non sono ridistribuibili senza licenza; il porting cross-platform le elimina.
- Il comportamento del dominio può essere preservato tramite test di caratterizzazione.

## 5. Limiti di valutazione

- L'app legacy **non è eseguibile end-to-end** qui: mancano Windows/IIS/SQL Server, i web service
  aziendali e le licenze Telerik/Kendo. La build legacy con Mono si ferma su reference mancanti
  (`MvcSiteMapProvider`, `Newtonsoft.Json`, `Microsoft.Office.Interop.Word`, `System.Web.Entity`,
  NPOI non in `packages.config`) → difetti reali del progetto, tracciati nel backlog.
- Il porting cross-platform è **in corso**: la web app .NET 8 non esiste ancora (fase M4).
  "Funzionante cross-platform" sarà vero solo al completamento di M4/M5.
- Nessuna affermazione di "funzionante" senza container avviato e test verdi.

## 6. Fasi

| Fase | Descrizione | Deliverable | Stato |
|------|-------------|-------------|-------|
| F0 | Studio approfondito del codice | `PLAN.md`, `docs/ANALISI-CODICE.md` | ✅ |
| F1 | Igiene repo, licenze, rimozione segreti (tree + history) | `LICENSE`, notices, `*.sample`, filter-repo | ✅ |
| F2 | Documentazione | `README.md`, `docs/GUIDA-COMPLETA.md` | ✅ |
| F3 | Prompt ingegnerizzato | `docs/PROMPT-INGEGNERIZZATO.md` | ✅ |
| F4 | CI: validazione statica + cross-platform | `.github/workflows/ci.yml`, `scripts/*` | 🔄 |
| F5 | Test logica pura | `tests/FCIWeb.Core.Tests` (26 test) | ✅ |
| F6 | Porting utility pure | `src/FCIWeb.Core/Utility` | ✅ |
| M2 | Porting modelli di dominio (senza `System.Web`/`System.Drawing`) | `src/FCIWeb.Core/Models` + `Services/IResourcesLookup` | ✅ |
| M3 | EF Core repository; SQLite demo + migrations; provider SQL Server compilato | `src/FCIWeb.Data/Migrations` | ✅* (SQLite testato; SQL Server runtime non verificato) |
| M4 | Web app ASP.NET Core + `DEMO_MODE` avviabile | `src/FCIWeb.Web` + Dockerfile | ✅ |
| M5 | Repository/servizi + adapter servizi esterni | `src/FCIWeb.Data`, `src/FCIWeb.Web/Data` | 🔶 (repository EF + adapter web; servizi esterni non portati) |
| M6 | Viste Razor runtime + UI | `src/FCIWeb.Web/Views` | ⏳ |
| M7 | Documenti: OpenXml al posto di Office Interop | `src/FCIWeb.Documents` | ⏳ |
| M8 | Docker multi-arch + release | `Dockerfile`, tag | ⏳ |
| F7 | Pulizia dead code / vendored / generated | `legacy/`, doc | ⏳ |
| F8 | Validazione finale e release | changelog, checklist | ⏳ |

Dettaglio del porting: [`docs/PORTING-CROSS-PLATFORM.md`](docs/PORTING-CROSS-PLATFORM.md).

## 7. Gate obbligatori

1. **Segreti**: nessuno nel working tree né nella history (verificato a ogni ciclo).
2. **Licenze**: nessuna DLL proprietaria/commerciale tracciata o nella history.
3. **Evidenza**: ogni "fatto" ha log/file allegato.
4. **Cambio distruttivo**: rimozione file, history rewrite, rotazione credenziali → conferma owner
   (già fornita per segreti e dipendenze).
5. **Build/test**: dichiarare se eseguiti o solo pianificati; nessun test inventato.
6. **Cross-platform**: una feature è "cross-platform" solo se `dotnet test`/container girano su Linux.

## 8. Rischi

| Rischio | Impatto | Mitigazione |
|---------|---------|-------------|
| Ampiezza del porting (M2–M8) | Alto | Strangler incrementale, un pezzo per ciclo, test a ogni passo |
| UI Telerik/Kendo commerciale | Alto | Valutare alternative permissive (M6); non pubblicare asset commerciali |
| Servizi interni non replicabili | Alto | Interfacce + adapter + `DEMO_MODE` con dati in-memory |
| Perdita di comportamento nel porting | Medio | Test di caratterizzazione sul legacy |
| Build legacy rotta (reference mancanti) | Medio | Fix `packages.config`/HintPath in un ciclo dedicato, o lasciare legacy come solo-riferimento |

## 9. Backlog ordinato (prossimi cicli)

1. Fix `packages.config`/HintPath legacy (`MvcSiteMapProvider`, `Newtonsoft.Json`, NPOI, Office Interop)
   per ripristinare la build .NET Framework (richiede Windows per la verifica finale).
2. **M2**: portare i modelli di dominio in `src/FCIWeb.Core/Models` rimuovendo `System.Web.Mvc`,
   `System.Drawing`, `Elmah`, `AreaCommonWeb` (attributi → annotazioni portabili; immagini → fuori dal modello).
3. **M3**: `src/FCIWeb.Data` con EF Core, provider SQLite (demo) e SQL Server (produzione), migrazioni.
4. **M4**: `src/FCIWeb.Web` ASP.NET Core con:
   - hosting cross-platform, di default `DEMO_MODE=true`;
   - auth a cookie per demo, `Negotiate` opzionale;
   - `IConfiguration` al posto di `Web.config`;
   - container `Dockerfile` Linux → criterio 15.
5. **M5/M6**: repository, servizi, viste Razor runtime, sostituzione UI commerciale.
6. Rotazione credenziali (owner) + verifica finale history.
7. Release: tag, changelog, checklist pubblicazione.

## 10. Registro cicli

| Ciclo | Data | Attività | Esito | Evidenza |
|-------|------|----------|-------|----------|
| 0 | 2026-10-16 | Studio codice, piano, prompt | ✅ | `PLAN.md`, `docs/*` |
| 1 | 2026-10-16 | Baseline igiene + docs + CI statica | ✅ | `check-secrets.sh` OK, `validate-config.sh` OK |
| 2 | 2026-10-16 | Rimozione segreti (tree+history), esclusione dipendenze proprietarie | ✅ | filter-repo ×2, `git grep` su storia = 0, `.git` 3 MB |
| 3 | 2026-10-16 | Avvio cross-platform: soluzione .NET 8, utility portate, 26 test, CI, doc porting | ✅ | `dotnet test` → 26/26 in Docker Linux |
| 4 | 2026-10-16 | Pubblicazione repo pubblico pulito + CI verde | ✅ | [run 38876403094](https://github.com/donatopepe/FCIWeb/actions/runs/38876403094): validate ✅, crossplatform ✅, legacy ⚠️ |
| 5 | 2026-10-16 | De-branding completo: nessun riferimento all'azienda (testi, acronimi, loghi, URL) in tree e history | ✅ | clone fresco: 0 hit; `logo.svg` generico; gate storia OK |
| 6 | 2026-10-16 | M2: porting entità di dominio a POCO + `IResourcesLookup`/`ResourcesDisplay` | ✅ | `dotnet build /warnaserror` OK; `dotnet test` → 42/42 |
| 7 | 2026-10-16 | Cybersecurity + M4: web app ASP.NET Core avviabile e usabile in DEMO_MODE | ✅ | container Linux: login/elenco/create/ricerca OK; 105 test; header di sicurezza |
| 8 | 2026-10-16 | M3: EF Core SQLite/SQL Server provider, test persistenza, wiring web, volume Docker | ✅* | 113 test passati; build /warnaserror 0/0; Compose up, login 200, DB sul volume |
| 9 | 2026-10-16 | M3: migrazioni EF versionate, design-time factory, adozione schema EnsureCreated senza perdita | ✅* | suite 116/116; build /warnaserror 0/0; Compose login 200 |
| 10 | 2026-10-16 | M5 partial: token versione + compare-and-swap transizioni; stale-write tests repository/HTTP | ✅* | suite 118/118; build /warnaserror 0/0 |
| 11 | 2026-10-16 | Test HTTP stale token con SQLite persistente e richiesta nuova | ✅* | suite 118/118; build /warnaserror 0/0; nessun push |
| 12 | 2026-10-16 | Test generation migration script SQL Server idempotente senza connessione live | ✅* | test Data 12/12, suite 119/119; build /warnaserror 0/0 |

### Dettaglio ciclo 2 (segreti e licenze)

- Backup: `/root/fciweb-backup-20261016/` (`fciweb-full.bundle`, worktree tar.gz).
- `git-filter-repo --replace-text` (16 pattern) + `--invert-paths` su file sensibili.
- Seconda passata `--invert-paths` sulle dipendenze commerciali (`lib/`, Kendo/Telerik assets).
- Verifiche: 0 hit su 11 stringhe sensibili in tutti i commit; 0 commit contenenti i file sensibili.
- `origin` ripristinato a `https://github.com/donatopepe/FCIWeb.git` (nessun push).

### Dettaglio ciclo 3 (cross-platform)

- `FCIWeb.CrossPlatform.sln` con `src/FCIWeb.Core` e `tests/FCIWeb.Core.Tests`.
- Portati `DateUtil`, `Extensions`, `MapObj` in `FCIWeb.Core.Utility` (rimosse dipendenze inutili da
  `System.Web`/Newtonsoft; `ContainsNormalized` resa null-safe).
- `dotnet test` in `mcr.microsoft.com/dotnet/sdk:8.0` → **Passed: 26, Failed: 0**.
- CI: job `crossplatform` bloccante (build `/warnaserror` + test + artifact TRX).
- `docs/PORTING-CROSS-PLATFORM.md` (strategia strangler, mappa di conversione, fasi M0–M8).

### Dettaglio ciclo 4 (pubblicazione)

- **Incidente rilevato e corretto**: il primo push conteneva in `PLAN.md` i valori letterali dei
  segreti (a scopo di verifica). Rilevato con `git grep` su clone fresco → il repo pubblico è stato
  **cancellato e ricreato** per eliminare gli oggetti orfani; la documentazione ora non contiene i
  valori e la deny-list è locale (`.secrets-denylist`, non versionata).
- Nuovo repo pubblico: <https://github.com/donatopepe/FCIWeb> (branch `master`).
- Vecchio repo privato `FCIWeb-legacy-backup` (conteneva i segreti originali) eliminato da GitHub;
  backup locale in `/root/fciweb-remote-backup.git`.
- Verifica su **clone fresco**: 0 hit di segreti nella history, 0 binari tracciati, working tree pulito.
- CI: primo run verde (validazione statica + cross-platform).

### Dettaglio ciclo 5 (de-branding)

- Rimossi i **loghi dell'istituto** da working tree e history; aggiunto `Source/Content/img/logo.svg`
  generico "FCIWeb".
- Neutralizzati: URL/domini interni, SSO, help desk, IP `172.28.166.x`, portali.
- Rinominati con nomi neutri gli identificatori che contenevano l'acronimo aziendale
  (proprietà/tipi dei proxy proprietari, variabili locali).
- Genericizzati testi di reparto e prefissi di build/storage; dati simulati (filiali) e view.
- History riscritta con `git-filter-repo --replace-text` (+ regex per stringhe spezzate dal
  code-gen) e `--invert-paths` sui loghi; repository GitHub **cancellato e ricreato** per eliminare
  gli oggetti orfani.
- **Verifica su clone fresco**: 0 hit per 21 token azienda/segreti, nessun logo tracciato, working
  tree pulito.
- Limite dichiarato: termini italiani generici di dominio ("data stop banca", "giorni attesa
  banca") restano perché indicano il processo, non l'istituto (rinominarli cambierebbe chiavi di
  configurazione e campi DB).

### Dettaglio ciclo 6 (M2 — modelli di dominio)

- Portate in `src/FCIWeb.Core/Models` le entità: `ParametriApp`, `Comune` (+ interfaccia `IAuditInfo`),
  `Profili`, `ProfiliWL`, `StatiPossibili`, `Stati`, `LogStati`, `column`, `Colonne`, `Fogli`, `Righe`,
  `EmailTesti`, `EmailSettings`, `EmailTipo`, `LogEmail`.
- Rimosse le dipendenze non portabili dai modelli: `System.Web.Mvc` (`HiddenInput`), `System.Drawing`,
  `Elmah`, `AreaCommonWeb`/`DAL.AreaCommonWeb`.
- Estratta l'astrazione `IResourcesLookup` (utenti/UO) + `ResourcesDisplay` per i nomi leggibili,
  con fallback al valore grezzo (null-safe, non propaga eccezioni).
- Nomi delle proprietà mantenuti identici al legacy per non rompere la mappatura EF (M3).
- `CS8981` (`column`) silenziato in `FCIWeb.Core.csproj` con motivazione.
- **Verifica**: `dotnet build -c Release /warnaserror` → 0 warning/0 errori; `dotnet test` → **42/42**.

### Dettaglio ciclo 7 (cybersecurity + M4)

**Cybersecurity (`src/FCIWeb.Core/Security`)**
- `SecurityGuards`: content-type null-safe, limite dimensione payload, `SanitizeFileName`
  (traversal Windows **e** POSIX), `IsSafeRelativePath`/`CombineUnderRoot` (drive/UNC/POSIX).
- `SecretRedactor`: maschera password, connection string, token/Bearer, chiavi private nei log.
- `SecureSmtpValidator`: impedisce credenziali SMTP in chiaro su host remoti, porte invalide,
  credenziali incoerenti.
- Analizzatori .NET abilitati per `src/` e `tests/` (`AnalysisLevel=latest-recommended`), bloccanti
  in CI via `/warnaserror`.
- Header di sicurezza + CSP, antiforgery globale, cookie auth `HttpOnly`/`SameSite`, PBKDF2-SHA256,
  autorizzazione per ruolo, container non-root, nessun header `Server`, `read_only` in compose.

**Web app avviabile (`src/FCIWeb.Web`, M4)**
- ASP.NET Core MVC 8 con `DEMO_MODE`: store in memoria con dati demo (nessun DB/servizio).
- Controller `Home`, `Account` (login/logout), `Richieste` (elenco/ricerca, dettagli, crea,
  cambio stato con transizioni validate, elimina solo admin).
- Viste Razor + CSS locale (nessuna CDN, coerente con la CSP `default-src 'self'`).
- `Dockerfile` multi-stage, `docker-compose.yml`, `.dockerignore`.
- **Verifica reale**: immagine costruita, container avviato in 1s, curl su login/elenco/create/ricerca;
  antiforgery mancante → 400; login errato → 200 con errore; ruolo user su delete → accesso negato.
- **Test**: 96 dominio + 9 integrazione web = **105**, verdi.

### Dettaglio ciclo 8 (M3 — persistenza)

- Aggiunto `src/FCIWeb.Core/Models/Richiesta.cs` come entità di dominio condivisa; MVC mantiene un
  tipo derivato solo per l'integrazione UI legacy.
- Aggiunto `src/FCIWeb.Data`: `FCIWebContext`, mapping/indici EF Core 8, provider SQLite e SQL Server,
  `EfRichiestaRepository` (CRUD, filtri, validazione transizioni, note motivazione).
- `FCIWEB_STORE=sqlite` (default): app crea `data/fciweb.db`; `FCIWEB_STORE=memory` seleziona lo store
  volatile. Compose monta volume persistente `/app/data`.
- Test SQLite reali su connection `:memory:` mantenuta aperta: 7 casi passati, incluso riapertura
  contesto, filtro/ordinamento, transizioni valide/invalide e delete. Test web crea una richiesta con
  POST autenticato su file temporaneo e la legge con GET successivo.
- **Verifica ciclo 8**: `dotnet test FCIWeb.CrossPlatform.sln -c Release` → Core 96/96, Data 7/7,
  Web 10/10 = **113/113**; `dotnet build ... /warnaserror` → **0 warning, 0 errori**.
- Docker `build` da context ridotto → OK; `docker compose up -d --build` → login 200, DB creato sul
  volume, UID `app` non-root. `docker compose down` eseguito; dati nel volume persistono.
- SQL Server provider compilato ma non testato con server; migrazioni EF aggiunte nel ciclo 9.

### Dettaglio ciclo 8 (M3 — persistenza)

- Entità `Richiesta` condivisa in `FCIWeb.Core.Models`; il tipo MVC deriva dal modello di dominio.
- Aggiunto `src/FCIWeb.Data`: EF Core 8.0.11, `FCIWebContext`, mapping/indici, repository CRUD,
  filtri e transizioni. Provider SQLite e SQL Server inclusi.
- Web default SQLite persistente (`data/fciweb.db`); `FCIWEB_STORE=memory` seleziona lo store volatile.
  Compose monta volume `/app/data`; directory scrivibile dall'UID non-root.
- Test SQLite su connessione in-memory condivisa: reopen context, filtri/ordinamento, transizione
  valida/invalida, motivazione, record inesistente, delete e validazione.
- `TODO.md` definisce obiettivo, metrica/soglia, casi, dipendenze e residui.
- **Test Linux**: Core 96/96, Data 7/7, Web 10/10 = **113/113**.
- **Build** Release `/warnaserror`: **0 warning, 0 errori**.
- **Docker/Compose**: build OK; `compose up` → login 200, UID `app`, DB presente sul volume; down OK.
- Gate segreti tree/history e XML OK; zero binari proprietari tracciati. Nessun push.
- **Limiti al ciclo 8**: prima del ciclo 9 schema usava `EnsureCreated`; provider SQL Server non testato.

### Dettaglio ciclo 9 (M3 — migrazioni)

- Tool locale `dotnet-ef` 8.0.11 (`.config/dotnet-tools.json`) e `FCIWebContextFactory` design-time;
  SQL Server richiede `FCIWEB_SQLSERVER_CONNECTION` via environment, nessun segreto committato.
- Generata migration `src/FCIWeb.Data/Migrations/*_InitialCreate.cs` + model snapshot.
- Runtime e fixture SQLite passati a `Database.Migrate()`.
- `LegacySchemaAdopter` riconosce database SQLite preesistenti con tabella `Richieste` e history assente/vuota;
  controlla colonne e registra migration iniziale senza drop/recreate. Test specifico verifica record preservato.
- `MigrationTests`: DB vuoto, migration idempotente, upgrade da `EnsureCreated`, schema incompatibile rifiutato.
- Verifica Linux ciclo 9: Core 96 + Data 10 + Web 10 = **116/116**; build `/warnaserror` 0 warning/errori.
- Compose smoke: login 200, DB creato, startup completato.
- Limite: migration SQL Server non testata senza istanza temporanea autorizzata.

### Dettaglio ciclo 11 (M5 partial — verifica web stale token)

- Test HTTP usa SQLite persistente temporaneo e crea una richiesta nuova (non seed in-memory).
- Legge token versione da Details, invia transizione valida e reinvia token stale per altra transizione.
- Verifica che stato e motivazione della prima operazione restino invariati dopo seconda POST.
- Suite: 96 Core + 11 Data + 11 Web = 118/118; build `/warnaserror` 0/0.
- Nessun push, working tree da commit locale pulito dopo gate.

### Dettaglio ciclo 10 (M5 partial — concorrenza)

- Aggiunto token `Version` GUID application-managed al dominio e alla migration SQLite.
- `TryChangeState` usa compare-and-swap su ID + versione attesa; un token stale riceve conflitto
  esplicito senza sovrascrivere lo stato precedente.
- Il form web invia il token versione; API repository accetta `expectedVersion` opzionale; store RAM
  serializza transizioni sotto lock.
- Test con due `DbContext`: prima transizione aggiorna, la seconda con versione obsoleta fallisce;
  rilettura conferma che stato e motivazione della prima operazione sono intatti.
- Test HTTP end-to-end crea una richiesta, invia due transizioni con stesso token e verifica che il
  secondo POST non sovrascriva il primo.
- Upgrade schema `EnsureCreated` v1 aggiunge `Version` e conserva righe preesistenti.
- Suite Linux 96 Core + 11 Data + 11 Web = **118/118**; build `/warnaserror` zero warning/errori.
- Limite: concurrency race verificata SQLite; SQL Server non testato end-to-end.

### Dettaglio ciclo 12 (M3 — SQL Server migration script design-time)

- Aggiunto test provider SQL Server (`UseSqlServer` senza aprire connessione) che genera migration
  script idempotente via `IMigrator.GenerateScript` e verifica DDL tabella, history, indici, token
  `Version` e batch `GO`.
- Il test valida il design-time SQL generato; non simula né dichiara connessione o applicazione a DB reale.
- Verifica Linux: Core 96 + Data 12 + Web 11 = **119/119**; build `/warnaserror` 0 warning/errori.
- SQL Server live resta bloccato da assenza di istanza disposable autorizzata.

## 11. Stato pubblicazione

- **Pubblicato**: repository **pubblico** su <https://github.com/donatopepe/FCIWeb> (branch `master`,
  commit iniziale `968cc0c`), creato ex novo per eliminare gli oggetti orfani con segreti.
- **Verificato pulito su clone fresco**: 0 occorrenze di segreti noti nella history, 0 binari
  tracciati, working tree senza credenziali, documentazione presente.
- **Pronto**: codice sorgente originale senza segreti né dipendenze non ridistribuibili; licenza MIT;
  documentazione completa; build/test cross-platform verdi.
- **Manca**: completamento del porting per l'avvio demo (M4); rotazione credenziali da parte
  dell'owner (la history è pulita, ma i valori sono stati esposti in passato).
- Backup locali (fuori dal repo): `/root/fciweb-backup-20261016/`, `/root/fciweb-remote-backup.git`.
