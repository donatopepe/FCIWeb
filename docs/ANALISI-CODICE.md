# Analisi del codice — FCIWeb

Studio del repository eseguito il 2026-10-16 sul branch `master` (commit `8c2454f`).
Tutti i numeri sono verificati con comandi shell sul working tree.

## 1. Cosa fa l'applicazione

FCIWeb è un'applicazione **intranet aziendale ASP.NET MVC 4** (area `FCI`) per la gestione di
"Richieste" e "Fogli" (strutture dati tabellari configurabili) con flusso a stati, allegati,
documenti Word/Excel ed email. Il dominio ruota attorno all'entità `Richieste`, identificata da
`NDG`/`Guid`, con UO (unità organizzative), filiali, profili di abilitazione e log di stato.

- Solution: `FCIWeb.sln` → unico progetto `Source/FCIWeb.csproj`.
- Entry point MVC: `Global.asax.cs` (`MvcApplication`), route di default
  `{controller}/{action}/{id}` → `Account/LogOnWithUtenzaTest`.
- Autenticazione: **Windows** (intranet), con override di test via `appSettings["UtenzaTest"]`.
- Framework: .NET Framework 4.x, MVC 4, EF 5 Code First, Razor 2 + RazorGenerator.

## 2. Mappa architetturale

```
Source/
├── Global.asax(.cs)          # bootstrap MVC, JsonModelBinder custom
├── App_Start/                # AuthConfig, BundleConfig, RazorGenerator, validation
├── Controllers/
│   └── AccountController.cs  # login/logout, cookie utenza, contesto AreaCommonWeb
├── Business/
│   └── AccountBusiness.cs
├── Areas/FCI/
│   ├── FCIAreaRegistration.cs
│   ├── Controllers/
│   │   ├── RichiestaController.cs       # lista, ricerca, griglia (Kendo)
│   │   ├── RichiestaMetodi.cs           # 67 KB: logica domanda, stati, allegati, documenti
│   │   ├── RichiestaControllerSimulato.cs
│   │   ├── UtenzaController.cs, DonatoController.cs, VincenzoController.cs, ComuneController.cs
│   ├── Models/
│   │   ├── TabelleDB.cs                 # entità EF: Richieste, Fogli, Righe, Colonne, Stati, LogStati, ...
│   │   └── ViewModel.cs, GestioneViewModel, EmailSettings
│   ├── DAL/
│   │   ├── FCIContext.cs                # DbContext "FCIContext"
│   │   └── InitialCreate.cs             # migrazione iniziale
│   ├── Repository/
│   │   ├── RCDRepository.cs / RisorseRepository / ResourcesRepository
│   ├── Uility/                          # (sic) utility: DateUtil, MapObj, EmailSender, Excel,
│   │                                    #  StampaDocumenti, FromFileHttp, ModelStateErrors, Extensions
│   └── Views/                           # 79 cshtml + 54 *.generated.cs (RazorGenerator)
├── Views/, Scripts/, Content/           # view di root e asset (molti vendorizzati)
├── App_GlobalResources/                 # localizzazioni Kendo grid/upload/datepicker (39 resx)
├── Migrations/
├── Web.config / connection.config / Web.{Debug,Release}.config
└── packages.config                      # 36 pacchetti NuGet
lib/                                     # DLL non-NuGet (proprietarie e commerciali)
```

## 3. Dipendenze

### 3.1 NuGet (`packages.config`, 36 pacchetti)

Componenti principali: MVC 4, Razor 2, WebPages, Web Optimization, EntityFramework 5,
Elmah + Elmah.MVC, DotNetOpenAuth (AspNet/Core/OAuth/OpenId), RazorGenerator.Mvc 2.2.2,
RazorEngine 3.3.0, Quartz 2.1.2, NPOI, DocumentFormat.OpenXml 2.7.1, ActionMailer 0.7.4,
jQuery/jQuery UI/Validation, WebGrease, Modernizr, CodeContracts.Unofficial, Glimpse.

> La cartella `packages/` **non è presente** nel repository (gitignored); serve `nuget restore`.

### 3.2 DLL in `lib/` (non NuGet) — **criticità distributiva**

| Assembly | Natura | Ridistribuibile? |
|----------|--------|------------------|
| `AreaCommonWeb.dll` | libreria aziendale proprietaria (Business/DAL/Models/Controllers base) | ❌ |
| `WebServiceModel.dll` | proxy web service aziendali (`WSSicurezza`, `WSProfiles`, ...) | ❌ |
| `Telerik.Web.Mvc.dll` | Telerik Extensions for ASP.NET MVC (commerciale, licenza a posto) | ❌ |
| `lib/KENDOUIMVC/2013.2.716.340`, `2013.2.918.340`, `2015.2.902` | Kendo UI for MVC (commerciale) | ❌ |
| `itextsharp.dll` | iTextSharp 5 (AGPL o licenza commerciale) | ⚠️ solo con AGPL/commerciale |
| `Microsoft.Office.Interop.Word.dll` | interoperabilità Office (richiede Word installato) | ⚠️ |
| `Microsoft.Web.Services3.dll` | WSE 3.0 (legacy, obsoleto) | ⚠️ |

**Conseguenza**: il repository così com'è **non è pubblicabile** con licenza MIT. Le DLL vanno
escluse dal repo pubblico e dichiarate come prerequisiti/licenze separate (vedi
`THIRD-PARTY-NOTICES.md`).

### 3.3 Servizi esterni a runtime

- `WebServiceModel.WSSicurezza.Service` (anagrafica utenze)
- `WebServiceModel.WSProfiles.Service` (profili)
- SQL Server `FILESERVER` (cataloghi `FCI`, `GRADIMENTO`, `Resources`)
- SMTP interno
- `AreaCommonWeb.*` per menu, contesto applicativo, repository risorse

Senza questi servizi l'app **non parte**: non è auto-contenuta.

## 4. Modello dati (EF 5 Code First)

`FCIContext : DbContext`, connection string `FCIContext`. `Database.SetInitializer<FCIContext>(null)`
→ **nessuna migrazione automatica**: lo schema va applicato manualmente (`InitialCreate.cs`).
Entità principali in `TabelleDB.cs`: `ParametriApp`, `EmailSettings`, `EmailTesti`, `LogEmail`,
`Fogli`, `Righe`, `Colonne`, `Stati`, `LogStati`, `Profili`, `ProfiliWL` (+ dominio `Richieste`
da `AreaCommonWeb`).

## 5. Problemi rilevati (debito tecnico e bloccanti)

### 5.1 Bloccanti per la pubblicazione
1. **Segreti in chiaro** in `connection.config`, `Web.config` (`Prod/Svi.WrapperBaseUserPassword`),
   `FCIWeb.csproj` (`PublishDatabaseSettings`), IP SMTP.
2. **DLL proprietarie/commerciali** committate in `lib/`.
3. **Dipendenza da infrastruttura interna** non replicabile pubblicamente.

### 5.2 Bug e fragilità
4. `elmah-sql` referenziato in `Web.config` ma assente in `connection.config`.
5. `JsonModelBinder` legge l'intero `Request.InputStream` senza limite di dimensione (rischio DoS) e
   assume `ContentType` non null (`contentType.Contains(...)` → NRE se `ContentType` è null).
6. `ProfileAugmentation`, `RichiestaControllerSimulato`, `StampaDocumenti_old`, `" - Old.cshtml"`,
   `old/` → codice morto/duplicato.
7. `catch { }` silenziosi (es. `FCIContext.SetProperty`) che nascondono errori.
8. `AccountController` dipende da `BaseController`/`GenericHelpers` di `AreaCommonWeb` → non testabile
   in isolamento.

### 5.3 Igiene repository
9. 54 viste `*.generated.cs` (output di build RazorGenerator) committate: normalmente gitignorate.
10. `Source/Scripts/ce-master` (+`.zip`), `Source/Scripts/x-spreadsheet-master`: interi progetti
    terzi vendorizzati (~1475 file) senza notices.
11. `Da aggiornare sul server.7z` (318 KB) e `Doc_Files/*.docx`: binari nel repo.
12. `old/` (55 file) copia storica divergente.
13. Nessuna licenza, nessun README di progetto, nessun test, nessuna CI.

## 6. Punti di forza

- Codice originale di dominio sostanzioso (~28.500 righe C#) e funzionante in origine.
- Uso di Dependency Injection manuale nei repository (`ResourcesRepository` iniettabile).
- Utility isolate e testabili (`DateUtil`, `Extensions`, `MapObj`).
- Localizzazioni già predisposte in `App_GlobalResources`.

## 7. Indicazioni per il risanamento

1. **Non cancellare**: archiviare `old/` in `legacy/` con un README che spiega le differenze.
2. **Segreti**: sostituire con `*.sample`; ruotare le credenziali; riscrivere la history.
3. **Licenze**: escludere `lib/` dal repo pubblico; documentare i prerequisiti.
4. **Testabilità**: estrarre la logica pura e aggiungere test; introdurre interfacce per i servizi
   esterni così da poter simulare in `DEMO_MODE`.
5. **Build**: tentare la compilazione in container (mono o .NET SDK + reference assemblies) e
   documentare il risultato reale.
6. **Hardening**: limitare la dimensione del body JSON, null-check, logging invece di `catch {}`.
