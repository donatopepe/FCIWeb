# PLAN — FCIWeb: risanamento, cross-platform e pubblicabilità

> Documento vivo. Ogni ciclo del loop aggiorna lo stato delle fasi e la tabella di avanzamento.
> Ultima revisione: 2026-10-16 (ciclo 3 — avvio porting cross-platform .NET 8)

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
| 11 | CI verde: validazione statica | 🔄 | workflow presente; run reale da fare (serve push) |
| 12 | CI verde: build + test cross-platform `.NET 8` | ✅* | `dotnet test` in Docker → 26/26 passati |
| 13 | Nessuna dipendenza proprietaria/commerciale tracciata | ✅ | `git ls-files` → nessun binario; history pulita |
| 14 | Dead code / vendored documentati o archiviati | ✅ | `docs/ANALISI-CODICE.md`, `docs/PORTING-CROSS-PLATFORM.md` |
| 15 | App cross-platform avviabile in `DEMO_MODE` (container Linux, no SQL/sei/terze parti) | ⏳ | fase M4 |
| 16 | `docs/PORTING-CROSS-PLATFORM.md` + piano di migrazione | ✅ | file |

\* criterio 12: test eseguiti localmente in container; il run in CI GitHub resta da attivare (criterio 11).

**Misurato al ciclo 3**: **13/16 = 0.8125** ✅ sopra soglia baseline.

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
| M2 | Porting modelli di dominio (senza `System.Web`/`System.Drawing`) | `src/FCIWeb.Core/Models` | ⏳ |
| M3 | EF Core + migrazioni + provider SQLite/SQL Server | `src/FCIWeb.Data` | ⏳ |
| M4 | Web app ASP.NET Core + `DEMO_MODE` | `src/FCIWeb.Web` | ⏳ |
| M5 | Repository/servizi + adapter servizi esterni | `src/FCIWeb.App` | ⏳ |
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

## 11. Stato pubblicazione

- **Pronto**: codice sorgente originale senza segreti né dipendenze non ridistribuibili; licenza MIT;
  documentazione completa; build/test cross-platform verdi.
- **Manca**: push su remoto (repo `donatopepe/FCIWeb` attualmente 404); rotazione credenziali da parte
  dell'owner; completamento porting per l'avvio demo; rimozione `old/` (fase F7).
