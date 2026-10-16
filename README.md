# FCIWeb

Applicazione **ASP.NET MVC 4 / .NET Framework** per la gestione intranet di *Richieste* e *Fogli*
strutturati: flusso a stati, allegati, documenti Word/Excel, notifiche email, profili di
abilitazione e log delle modifiche.

> ⚠️ **Stato del progetto**: applicazione legacy di origine aziendale, pubblicata come codice
> sorgente a scopo di studio/manutenzione. Richiede **Windows + IIS + SQL Server** e dipendenze di
> terze parti **non ridistribuibili** (Telerik, Kendo UI, `AreaCommonWeb.dll`, `WebServiceModel.dll`).
> Vedi [Limiti noti](#limiti-noti) e [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md).

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

1. **Non è auto-contenuta**: richiede servizi interni (`WSSicurezza`, `WSProfiles`), SQL Server e
   SMTP. Non è avviabile "a vuoto" da un utente esterno.
2. **Dipendenze non ridistribuibili**: `lib/` contiene DLL proprietarie e commerciali. Il codice
   originale è MIT, ma l'applicazione completa **non** è utilizzabile senza acquisire quelle licenze
   (vedi [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md)).
3. **Build non verificata su Linux**: il progetto è .NET Framework e dipende da `System.Web`.
4. **Nessuna suite di test** al momento.
5. **Codice morto/duplicato** presente (`old/`, `*Simulato*`, `*_old*`, `*.generated.cs`).

## Sicurezza

- **Non inserire credenziali reali** in file versionati: usa i `*.sample` e `.gitignore`.
- Le credenziali che erano presenti nella history **devono essere considerate compromesse e ruotate**.
- Per segnalazioni: vedi [`SECURITY.md`](SECURITY.md).

## Licenza

Codice originale: [MIT](LICENSE) © 2023-2026 Donato Pepe.
Le dipendenze di terze parti restano soggette alle proprie licenze
([`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md)).
