# PLAN — Risanamento, documentazione e pubblicabilità di FCIWeb

> Documento vivo. Ogni ciclo del loop aggiorna lo stato delle fasi e la tabella di avanzamento.
> Ultima revisione: 2026-10-16 (ciclo 0 — studio iniziale)

## 1. Obiettivo in una frase

Rendere il repository **FCIWeb** un progetto legacy ASP.NET MVC 4 **compreso, documentato, ripulito da segreti e dipendenze non distribuibili, con licenza MIT, build/verifica riproducibile e guide complete**, così che possa essere pubblicato e compreso da terzi.

## 2. Metrica e soglia (rubrica)

**Metrica**: punteggio di rubrica = criteri soddisfatti / 12 (ogni criterio 0/1, con evidenza allegata).

**Soglia baseline**: ≥ 0.75 (9/12).
**Soglia obiettivo (chiusura)**: ≥ 0.92 (11/12).
**Misurato al ciclo 1 (2026-10-16)**: **10/12 = 0.833** ✅ sopra soglia baseline.

| # | Criterio | Peso | Stato | Evidenza |
|---|----------|------|-------|----------|
| 1 | `README.md` completo (scopo, architettura, requisiti, build, run, config, licenza, limiti) | 1 | ✅ | `README.md` |
| 2 | `docs/GUIDA-COMPLETA.md` (setup, DB, IIS, config, deploy, troubleshooting, FAQ) | 1 | ✅ | `docs/GUIDA-COMPLETA.md` |
| 3 | `LICENSE` MIT sul solo codice originale | 1 | ✅ | `LICENSE` |
| 4 | `THIRD-PARTY-NOTICES.md` con analisi licenze di tutte le dipendenze | 1 | ✅ | `THIRD-PARTY-NOTICES.md` |
| 5 | Nessun segreto nel working tree (DB, SMTP, appSettings sensibili) | 1 | ✅ | `scripts/check-secrets.sh` → OK |
| 6 | Template di configurazione (`*.sample`) + `.gitignore` che esclude i file reali | 1 | ✅ | `connection.config.sample`, `.gitignore`, `git check-ignore` OK |
| 7 | `docs/ANALISI-CODICE.md` con mappa architetturale e debito tecnico | 1 | ✅ | `docs/ANALISI-CODICE.md` |
| 8 | `PLAN.md` con fasi, stati e gate | 1 | ✅ | questo file |
| 9 | `docs/PROMPT-INGEGNERIZZATO.md` | 1 | ✅ | `docs/PROMPT-INGEGNERIZZATO.md` |
| 10 | CI (`.github/workflows/ci.yml`) esegue validazione config + build tentata | 1 | 🔄 | workflow presente; **nessun run verde** (manca push) → non conta |
| 11 | Suite di test per la logica pura eseguibile con evidenza | 1 | ⏳ | non ancora implementata |
| 12 | Dead code / vendored dir documentati o archiviati | 1 | ✅ | documentati in `docs/ANALISI-CODICE.md` §5.3 e backlog §9 |

> Nota: i criteri 10 e 11 richiedono un runner. Su questa macchina Linux non esiste .NET Framework: la build reale va tentata in container/CI. Se non è possibile compilare, il criterio 10 può essere soddisfatto solo da validazione statica e il limite va dichiarato (non spacciato per build verde).

## 3. Fatti verificati (ciclo 0)

- Progetto: ASP.NET MVC 4 / .NET Framework 4.x, Entity Framework 5 Code First, Razor, area `FCI`.
- Dipendenze proprietarie/commerciali **non ridistribuibili** in `lib/`: `AreaCommonWeb.dll`, `WebServiceModel.dll`, `Telerik.Web.Mvc.dll`, `KENDOUIMVC` (3 versioni), `itextsharp.dll` (AGPL), `Microsoft.Office.Interop.Word.dll`.
- Servizi interni richiamati a runtime: `WSSicurezza`, `WSProfiles` (web service aziendali) → l'app **non è eseguibile pubblicamente** senza di essi.
- Segreti in chiaro nel working tree: `Source/connection.config` (×2), commenti in `Source/Web.config`, `appSettings` (`Prod/Svi.WrapperBaseUserPassword`), `Source/FCIWeb.csproj` (`PublishDatabaseSettings`), IP SMTP interno.
- 54 viste `*.generated.cs` (RazorGenerator) committate; 1475 file sotto `Source/Scripts`, 859 sotto `Source/Content` (molti vendorizzati).
- `old/` (55 file) è una copia storica divergente del codice dell'area.
- `elmah-sql` è referenziato in `Web.config` ma non definito in `connection.config` → bug latente.
- Repo remoto `origin` = `github.com/donatopepe/FCIWeb.git` (attualmente 404 → privato/rimosso).
- Autore unico: Donato Pepe <[PRIVATE_EMAIL]>. Ultimo commit: 2023-01-16.

## 4. Ipotesi

- I segreti presenti sono credenziali reali di un ambiente aziendale: vanno **ruotate** dall'owner, non solo rimosse.
- Le DLL in `lib/` non possono essere pubblicate senza licenza; vanno escluse dal repo pubblico e documentate come prerequisiti.
- L'utente vuole pubblicare il **codice originale** sotto MIT, non le dipendenze di terzi.

## 5. Limiti di valutazione

- Non è possibile eseguire l'app end-to-end qui: mancano Windows, IIS, SQL Server, i web service aziendali e le licenze Telerik/Kendo.
- "Completamente funzionante" è quindi declinato come: **(a)** build/validazione riproducibile dove possibile, **(b)** comprensibilità e installabilità documentate, **(c)** assenza di blocchi alla pubblicazione (segreti, licenze). Il funzionamento end-to-end resta un gate esterno (owner + ambiente Windows).
- Eventuali affermazioni di "test passati" valgono solo per ciò che è realmente eseguito, con log allegato.

## 6. Fasi e stato

| Fase | Descrizione | Deliverable | Stato |
|------|-------------|-------------|-------|
| F0 | Studio approfondito del codice | questo piano + `docs/ANALISI-CODICE.md` | ✅ fatto |
| F1 | Igiene repo, licenze, rimozione segreti | `LICENSE`, `THIRD-PARTY-NOTICES.md`, `*.sample`, `.gitignore`, `SECURITY.md` | ✅ baseline fatto |
| F2 | Documentazione | `README.md`, `docs/GUIDA-COMPLETA.md` | ✅ baseline fatto |
| F3 | Prompt ingegnerizzato | `docs/PROMPT-INGEGNERIZZATO.md` | ✅ fatto |
| F4 | Build/verifica riproducibile + CI | `.github/workflows/ci.yml`, script di build | 🔄 in corso |
| F5 | Test logica pura | progetto di test + evidenza | ⏳ da fare |
| F6 | Pulizia dead code / vendored | `legacy/` archivio, documentazione scelte | ⏳ da fare |
| F7 | Distribuibilità (dipendenze proprietarie) | adapter/astrazioni + `DEMO_MODE`, oppure esclusione `lib/` | ⏳ da fare |
| F8 | Validazione finale e release | tag, changelog, checklist pubblicazione | ⏳ da fare |

## 7. Gate obbligatori

1. **Gate segreti**: nessun segreto nel working tree né nella history destinata alla pubblicazione (serve `git filter-repo`/BFG, da eseguire solo dopo backup e conferma owner).
2. **Gate licenze**: le DLL proprietarie/commerciali non sono nel repo pubblico; `THIRD-PARTY-NOTICES.md` esaustivo.
3. **Gate evidenza**: ogni "fatto" ha log/file allegato. Niente affermazioni non verificate.
4. **Gate cambio distruttivo**: rimozione file, riscrittura history, rotazione credenziali → richiedono conferma umana.
5. **Gate build**: dichiarare esplicitamente se la build è stata eseguita o solo simulata.

## 8. Rischi

| Rischio | Impatto | Mitigazione |
|---------|---------|-------------|
| Credenziali già compromesse (repo pubblico in passato) | Alto | Rotazione lato owner + history rewrite |
| Licenze Telerik/Kendo/iTextSharp | Alto (legale) | Esclusione `lib/`, notices, valutazione sostituzione |
| App non eseguibile senza servizi interni | Alto (funzionalità) | `DEMO_MODE` con adapter mock, o dichiarare il limite |
| Build non riproducibile su Linux | Medio | Container Windows in CI o build locale documentata |
| Ampiezza scope | Medio | Fasi incrementali, un cambiamento per ciclo |

## 9. Backlog ordinato (prossimi cicli)

1. ~~Baseline F1/F2 (licenza, notices, segreti, template, README, guida)~~ ✅ ciclo 1.
2. CI: primo run verde del job `validate` (richiede push su un remoto o `act`); build in container
   `mono:6.12.0` o `dotnet/sdk` + reference assemblies, con esito reale dichiarato.
3. Test unitari per `DateUtil`, `Extensions`, `MapObj`, `ModelStateErrors` (dove disaccoppiabili),
   con log allegato.
4. Archivio `old/` → `legacy/` con README delle differenze.
5. Valutazione sostituzione Telerik/Kendo/iTextSharp e rimozione `Microsoft.Office.Interop.Word`
   (alternativa OpenXML già presente: `DocumentFormat.OpenXml`).
6. Hardening `JsonModelBinder` (limite dimensione body, null-check `ContentType`), `catch {}` silenziosi,
   ELMAH `allowedUsers`.
7. Spostamento `lib/` fuori dal repo pubblico + aggiornamento `.csproj` con `HintPath` a `lib/`
   documentato; verifica che il repo pubblico non contenga DLL proprietarie.
8. Rotazione credenziali (owner) + riscrittura history (`git filter-repo`) con backup.
9. Definizione di `DEMO_MODE` (persistenza in-memory + utente fittizio) per run pubblico.
10. Release: tag, changelog, checklist pubblicazione.

## 10. Registro cicli (task ledger)

| Ciclo | Data | Attività | Esito | Evidenza |
|-------|------|----------|-------|----------|
| 0 | 2026-10-16 | Studio codice, piano, prompt ingegnerizzato | ✅ | `PLAN.md`, `docs/*` |
| 1 | 2026-10-16 | Baseline F1/F2: licenza, notices, segreti rimossi, template, README, guida, CI, script | ✅ | `check-secrets.sh` OK, `validate-config.sh` OK, rubrica 10/12 |
| 2 | — | — | — | — |

### Dettaglio ciclo 1 — evidenze

- **Segreti rimossi** da `Source/connection.config` (ora `*.sample`), `Source/Web.config`
  (`ServerSmtp`, `UtenzaTest`, `Prod/Svi.WrapperBaseUser*`), `Source/FCIWeb.csproj`
  (`PublishDatabaseSettings`), commento credenziali ELMAH in `Web.config`.
  Backup dei file originali: `/root/fciweb-secrets-backup/` (fuori dal repo, permessi 600).
- **Tracking rimosso** (file preservati su disco): `Source/connection.config`,
  `Source/Doc_Files/**`, `Da aggiornare sul server.7z`.
- **Verifica**: `bash scripts/check-secrets.sh` → `OK: nessun segreto evidente nel working tree.`
- **XML**: `bash scripts/validate-config.sh` → 8 file OK (incluso `connection.config.sample`).
- **YAML**: `.github/workflows/ci.yml` validato con `yaml.safe_load` → OK.
