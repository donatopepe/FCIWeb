# FCIWeb

Applicazione **ASP.NET MVC 4 / .NET Framework** per la gestione intranet di *Richieste* e *Fogli*
strutturati: flusso a stati, allegati, documenti Word/Excel, notifiche email, profili di
abilitazione e log delle modifiche.

> ⚠️ **Stato del progetto**: applicazione legacy di origine aziendale, in corso di **porting a .NET 8
> cross-platform** (Linux/macOS/Windows). Il codice legacy (`Source/`) resta come riferimento; il
> nuovo codice è in `src/`. Vedi [`docs/PORTING-CROSS-PLATFORM.md`](docs/PORTING-CROSS-PLATFORM.md)
> e [`PLAN.md`](PLAN.md).

## Cross-platform (.NET 8) — app avviabile e usabile

La soluzione **`FCIWeb.CrossPlatform.sln`** compila, testa ed **esegue** su Linux, macOS e Windows,
**senza** SQL Server, senza servizi interni e senza dipendenze proprietarie (DEMO_MODE con dati in
memoria).

```bash
# richiede .NET SDK 8+
dotnet run --project src/FCIWeb.Web            # http://localhost:5000 (o porta indicata)
```

Oppure con Docker (immagine non-root, porta 8080):

```bash
docker compose up --build                      # http://localhost:8080
# oppure
docker build -t fciweb-web:local -f src/FCIWeb.Web/Dockerfile .
docker run --rm -p 8080:8080 fciweb-web:local
```

**Credenziali demo** (solo locali, sovrascrivibili con `Demo:Users`):
`admin / Admin123!` (ruolo admin) — `user / User123!` (ruolo user).

Cosa puoi fare: autenticarti, sfogliare e cercare le richieste, crearne una, cambiarne lo stato
(flusso a stati con transizioni validate), eliminarla (solo admin).

Sicurezza già attiva: header di sicurezza + CSP, antiforgery su tutte le POST, password verificate
con PBKDF2-SHA256, autorizzazione per ruolo, container non-root, nessun header `Server`.

Test:

```bash
dotnet test FCIWeb.CrossPlatform.sln -c Release   # 105 test (dominio + integrazione web)
```

Stato: `src/FCIWeb.Core` (dominio + sicurezza) e `src/FCIWeb.Web` (ASP.NET Core MVC) funzionanti in
demo; restano EF Core (M3) e il porting completo delle viste legacy (M6). Dettagli in
[`PLAN.md`](PLAN.md) e [`docs/PORTING-CROSS-PLATFORM.md`](docs/PORTING-CROSS-PLATFORM.md).

---

## Indice

- [Panoramica](#panoramica)
- [Architettura](#architettura)
- [Requisiti](#requisiti)
- [Installazione rapida](#installazione-rapida)
- [Configurazione](#configurazione)
- [Build e avvio](#build-e-avvio)
- [Test](#test)
- [Documentazione](#documentazione)
- [Limiti noti](#limiti-noti)
- [Sicurezza](#sicurezza)
- [Licenza](#licenza)

---

## Panoramica

| Voce | Valore |
|------|--------|
| Tipo | Web app intranet (server-rendered) |
| Framework | ASP.NET MVC 4, .NET Framework 4.x |
| ORM | Entity Framework 5 (Code First) |
| Autenticazione | Windows Authentication |
| UI | Razor views + Telerik MVC + Kendo UI |
| Area di dominio | `Source/Areas/FCI` (Richieste, Fogli, Stati, Allegati, Documenti) |
| Entry point | `Source/Global.asax` → route default `Account/LogOnWithUtenzaTest` |

Il modulo applicativo principale è l'area `FCI`:

- **Richieste**: creazione, filtro, gestione a stati, cambio stato, log.
- **Fogli / Righe / Colonne**: tabelle configurabili per l'utente.
- **Allegati e Documenti**: upload/download, generazione documenti Word/Excel.
- **Email**: template e log invii (`EmailSettings`, `EmailTesti`, `LogEmail`).
- **Profili e UO**: abilitazioni per profilo e unità organizzativa.

## Architettura

```
FCIWeb.sln
└── Source/FCIWeb.csproj
    ├── Global.asax(.cs)         bootstrap MVC + JsonModelBinder
    ├── App_Start/               AuthConfig, BundleConfig, RazorGenerator, validation
    ├── Controllers/AccountController.cs
    ├── Business/AccountBusiness.cs
    ├── Areas/FCI/
    │   ├── Controllers/         RichiestaController, RichiestaMetodi, Utenza, ...
    │   ├── Models/              entità EF + view model
    │   ├── DAL/                 FCIContext (DbContext), InitialCreate
    │   ├── Repository/          accesso dati
    │   ├── Uility/              DateUtil, MapObj, EmailSender, Excel, StampaDocumenti, ...
    │   └── Views/               viste Razor (+ *.generated.cs da RazorGenerator)
    ├── Scripts/, Content/       asset front-end (molti vendorizzati)
    └── Web.config, connection.config
lib/                             DLL di terze parti / proprietarie (NON ridistribuibili)
old/                             copia storica divergente del codice (archivio)
```

Dettaglio completo: [`docs/ANALISI-CODICE.md`](docs/ANALISI-CODICE.md).

## Requisiti

### Runtime (produzione)

- Windows Server con **IIS 7+**, **Windows Authentication** abilitata, anonymous disabilitata.
- **.NET Framework 4.5+**.
- **SQL Server** con i cataloghi dell'applicazione.
- Accesso ai web service aziendali `WSSicurezza` / `WSProfiles`.
- SMTP raggiungibile.

### Sviluppo

- Visual Studio 2019+ (workload *ASP.NET e sviluppo web*).
- NuGet (`nuget restore`).
- Le DLL in `lib/` fornite dall'owner (non incluse nel repo pubblico).
- Office (per `Microsoft.Office.Interop.Word`) se si generano documenti Word.

> 💡 **Non esiste oggi un percorso di build/esecuzione su Linux documentato e verificato.** Vedi
> [`docs/GUIDA-COMPLETA.md`](docs/GUIDA-COMPLETA.md).

## Installazione rapida

```bash
git clone https://github.com/donatopepe/FCIWeb.git
cd FCIWeb

# 1. Ripristina i pacchetti NuGet
nuget restore FCIWeb.sln

# 2. Prepara la configurazione locale (i file reali NON sono versionati)
cp Source/connection.config.sample Source/connection.config
cp Source/Web.config.sample Source/Web.config   # se presente
# ... compila i placeholder con i TUOI dati (server, DB, credenziali, SMTP)

# 3. Apri la solution in Visual Studio
start FCIWeb.sln
```

Guida passo-passo (IIS, database, deploy, troubleshooting):
[`docs/GUIDA-COMPLETA.md`](docs/GUIDA-COMPLETA.md).

## Configurazione

Nessun segreto è versionato. I file reali sono esclusi via `.gitignore` e vanno creati dai template:

| File reale (ignorato) | Template versionato | Contenuto |
|-----------------------|---------------------|-----------|
| `Source/connection.config` | `Source/connection.config.sample` | Connection string SQL Server |
| `Source/Web.config` (appSettings) | `Source/Web.config.sample` | `ServerSmtp`, `UtenzaTest`, wrapper user/password |

Chiavi `appSettings` sensibili da valorizzare localmente:

- `ServerSmtp` — host del server di posta.
- `UtenzaTest` — matricola utente per la profilazione di test (opzionale).
- `Prod.WrapperBaseUserName` / `Prod.WrapperBaseUserPassword` — credenziali wrapper ambiente di produzione.
- `Svi.WrapperBaseUserName` / `Svi.WrapperBaseUserPassword` — credenziali wrapper ambiente di sviluppo.

Variabili d'ambiente alternative sono documentate nella guida.

## Build e avvio

In Visual Studio:

1. Imposta `Source` come progetto di avvio.
2. Seleziona IIS Express con **Windows Authentication = Enabled**, **Anonymous = Disabled**.
3. Avvia (F5). La route di default apre la pagina di logon.

Da riga di comando (Windows, con MSBuild):

```powershell
nuget restore FCIWeb.sln
msbuild FCIWeb.sln /p:Configuration=Release /p:Platform="Any CPU"
```

Lo stato reale della build riproducibile in CI è tracciato in [`PLAN.md`](PLAN.md).

## Test

Il repository **non contiene ancora** una suite di test eseguibile. L'aggiunta di unit test per la
logica pura (`DateUtil`, `Extensions`, `MapObj`, `ModelStateErrors`) e la loro esecuzione sono
tracciate come fase **F5** in [`PLAN.md`](PLAN.md) con evidenza dei log.

## Documentazione

| Documento | Contenuto |
|-----------|-----------|
| [`PLAN.md`](PLAN.md) | Obiettivo, metrica, fasi, gate, stato, backlog |
| [`docs/ANALISI-CODICE.md`](docs/ANALISI-CODICE.md) | Studio approfondito del codice e debito tecnico |
| [`docs/GUIDA-COMPLETA.md`](docs/GUIDA-COMPLETA.md) | Setup, database, IIS, deploy, troubleshooting |
| [`docs/PROMPT-INGEGNERIZZATO.md`](docs/PROMPT-INGEGNERIZZATO.md) | Prompt di lavoro per gli agenti |
| [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md) | Licenze delle dipendenze |
| [`SECURITY.md`](SECURITY.md) | Policy di sicurezza e gestione segreti |
| [`CONTRIBUTING.md`](CONTRIBUTING.md) | Come contribuire |

## Limiti noti

1. **Il legacy non è auto-contenuto**: richiede servizi interni (`WSSicurezza`, `WSProfiles`),
   SQL Server e SMTP, e gira solo su .NET Framework/Windows.
2. **Dipendenze non ridistribuibili**: le DLL proprietarie/commerciali (`AreaCommonWeb`,
   `WebServiceModel`, Telerik, Kendo, iTextSharp, WSE3) **non sono nel repository** (né nella
   history). Vedi [`lib/README.md`](lib/README.md) e [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md).
3. **Porting in corso**: la web app .NET 8 (`src/FCIWeb.Web`) non esiste ancora; oggi il
   cross-platform copre libreria di dominio + test (fase M4 di `PLAN.md`).
4. **Build legacy non verificata qui**: la build .NET Framework con Mono si ferma su reference
   mancanti (`MvcSiteMapProvider`, `Newtonsoft.Json`, Office Interop, NPOI) — difetti tracciati in
   `PLAN.md` §9.
5. **Codice morto/duplicato** presente (`old/`, `*Simulato*`, `*_old*`, `*.generated.cs`).

## Sicurezza

- **Non inserire credenziali reali** in file versionati: usa i `*.sample` e `.gitignore`.
- Le credenziali che erano presenti nella history **devono essere considerate compromesse e ruotate**.
- Per segnalazioni: vedi [`SECURITY.md`](SECURITY.md).

## Licenza

Codice originale: [MIT](LICENSE) © 2023-2026 Donato Pepe.
Le dipendenze di terze parti restano soggette alle proprie licenze
([`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md)).
