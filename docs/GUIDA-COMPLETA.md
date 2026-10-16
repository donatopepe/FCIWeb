# Guida completa — FCIWeb

Guida operativa per installare, configurare, avviare, distribuire e diagnosticare FCIWeb.
Riferimenti: [`README.md`](../README.md), [`docs/ANALISI-CODICE.md`](ANALISI-CODICE.md),
[`PLAN.md`](../PLAN.md).

> **Nota importante.** FCIWeb è un'applicazione .NET Framework 4.x che dipende da Windows, IIS,
> SQL Server, da web service aziendali interni (`WSSicurezza`, `WSProfiles`) e da DLL di terze
> parti non ridistribuibili. La guida descrive il percorso **ufficiale e verificato** su Windows.
> Un percorso Linux/CI è in fase di valutazione ed è tracciato in `PLAN.md` (fase F4).

---

## Indice

1. [Prerequisiti](#1-prerequisiti)
2. [Ottenere il codice](#2-ottenere-il-codice)
3. [Configurazione](#3-configurazione)
4. [Database](#4-database)
5. [Avvio in sviluppo (IIS Express)](#5-avvio-in-sviluppo-iis-express)
6. [Deploy in IIS](#6-deploy-in-iis)
7. [Build](#7-build)
8. [Test](#8-test)
9. [Troubleshooting](#9-troubleshooting)
10. [FAQ](#10-faq)
11. [Glossario](#11-glossario)

---

## 0. Avvio rapido cross-platform (consigliato)

Il modo più semplice per **vedere l'app funzionante** (Linux/macOS/Windows, senza database):

```bash
dotnet run --project src/FCIWeb.Web        # http://localhost:5000
# oppure
docker compose up --build                  # http://localhost:8080
```

Credenziali demo (solo locali): `admin / Admin123!` — `user / User123!`.
Database SQLite persistente: `data/fciweb.db`; schema creato/aggiornato con migrazioni EF Core all'avvio.
Per SQL Server, il provider genera script EF Core; applicazione runtime non ancora verificata.
In Docker Compose è montato nel volume `fciweb-data`.
Per usare lo store volatile in memoria, impostare `FCIWEB_STORE=memory`.
Sovrascrivibili con la sezione di configurazione `Demo:Users` o variabili d'ambiente
(`Demo__Users__0__Username`, `Demo__Users__0__Password`, `Demo__Users__0__Roles`).

Test:

```bash
dotnet test FCIWeb.CrossPlatform.sln -c Release      # 119 test
```

Le sezioni seguenti (1–11) descrivono invece il **legacy .NET Framework/IIS**, utile per capire il
sistema originale e per un deploy on-premise. Vedi anche
[`PORTING-CROSS-PLATFORM.md`](PORTING-CROSS-PLATFORM.md).

## 1. Prerequisiti

### 1.1 Sistema e runtime

| Componente | Versione | Note |
|-----------|----------|------|
| Windows | 10/11 o Windows Server 2016+ | richiesto da `System.Web` |
| .NET Framework | 4.5 o superiore | target del progetto |
| IIS | 7+ con ASP.NET 4 | per il deploy |
| SQL Server | 2012+ | cataloghi applicativi |
| Visual Studio | 2019+ | workload *ASP.NET e sviluppo web* |
| NuGet CLI | 5+ | `nuget restore` |
| MSBuild | 15+ (incluso in VS) | build da riga di comando |

### 1.2 Dipendenze non incluse nel repository pubblico

Le seguenti DLL devono essere fornite dall'owner (non ridistribuibili):

- `lib/AreaCommonWeb.dll`
- `lib/WebServiceModel.dll`
- `lib/Telerik.Web.Mvc.dll`
- `lib/KENDOUIMVC/<versione>/Kendo.Mvc.dll`
- `lib/itextsharp.dll` (verificare licenza)
- `lib/Microsoft.Office.Interop.Word.dll` (o Office installato)
- `lib/Microsoft.Web.Services3.dll`

Vedi [`THIRD-PARTY-NOTICES.md`](../THIRD-PARTY-NOTICES.md).

### 1.3 Servizi esterni

| Servizio | Uso | Configurazione |
|----------|-----|----------------|
| `WSSicurezza` | anagrafica utenze | URL nei settings / `lib/WebServiceModel.dll` |
| `WSProfiles` | profili utente | come sopra |
| SQL Server | dati applicativi | `connection.config` |
| SMTP | invio email | `appSettings["ServerSmtp"]` |

## 2. Ottenere il codice

```bash
git clone https://github.com/donatopepe/FCIWeb.git
cd FCIWeb
nuget restore FCIWeb.sln
```

Se il restore fallisce perché `packages/` è assente, assicurati che `nuget` sia nel `PATH` e che
`Source/packages.config` sia raggiungibile.

## 3. Configurazione

### 3.1 File di configurazione

| File | Versionato | Scopo |
|------|-----------|-------|
| `Source/Web.config` | ✅ (senza segreti) | configurazione MVC, binding, appSettings non sensibili |
| `Source/connection.config` | ❌ (solo `.sample`) | connection string SQL Server |
| `Source/Web.Debug.config` / `Web.Release.config` | ✅ | trasformazioni di build |
| `Source/wse3policyCache.config` | ✅ | policy WSE 3.0 |

### 3.2 Connection string

Crea `Source/connection.config` a partire da `Source/connection.config.sample`:

```xml
<connectionStrings>
  <add name="FCIContext"
       connectionString="Data Source=<SERVER>;Initial Catalog=FCI;User ID=<USER>;Password=<PASSWORD>"
       providerName="System.Data.SqlClient" />
  <add name="ApplicationServices"
       connectionString="Data Source=<SERVER>;Database=FCI;Trusted_Connection=False;MultipleActiveResultSets=true;User ID=<USER>;Password=<PASSWORD>"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

> In alternativa usa `Integrated Security=True` con l'identità del pool applicativo IIS.

### 3.3 `appSettings` sensibili

In `Source/Web.config` (o override locale) valorizza:

```xml
<add key="ServerSmtp" value="<SMTP_HOST>" />
<add key="UtenzaTest" value="<MATRICOLA_TEST>" />            <!-- opzionale -->
<add key="Prod.WrapperBaseUserName" value="<WRAPPER_USER>" />
<add key="Prod.WrapperBaseUserPassword" value="<WRAPPER_PWD>" />
<add key="Svi.WrapperBaseUserName" value="<WRAPPER_USER>" />
<add key="Svi.WrapperBaseUserPassword" value="<WRAPPER_PWD>" />
```

> ⚠️ Non committare mai questi valori reali. Vedi [`SECURITY.md`](../SECURITY.md).

### 3.4 Autenticazione Windows

L'app usa l'autenticazione Windows. In `Web.config` verifica:

```xml
<authentication mode="Windows" />
<authorization>
  <deny users="?" />
</authorization>
```

## 4. Database

`FCIContext` usa `Database.SetInitializer<FCIContext>(null)`: **nessuna creazione/migrazione
automatica**. Lo schema va creato manualmente.

### 4.1 Creazione schema

1. Crea il database `FCI` (e gli altri cataloghi richiesti: `GRADIMENTO`, `Resources`).
2. Applica lo schema iniziale da `Source/Areas/FCI/DAL/InitialCreate.cs` (migrazione EF) oppure
   ripristina un backup fornito dall'owner.

Da Package Manager Console (Visual Studio):

```powershell
Enable-Migrations -ContextTypeName FCIWeb.Areas.FCI.DAL.FCIContext -Force
Update-Database -ConnectionStringName FCIContext
```

> Poiché l'inizializzatore è `null`, EF non dropperà né ricreerà lo schema: `Update-Database`
> applica solo le migrazioni presenti.

### 4.2 Verifica

```sql
SELECT TOP 10 * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = 'FCI';
```

## 5. Avvio in sviluppo (IIS Express)

1. Apri `FCIWeb.sln` in Visual Studio.
2. Imposta `Source` come progetto di avvio.
3. Seleziona il profilo **IIS Express**.
4. Nel pannello Proprietà del progetto:
   - *Anonymous Authentication* → **Disabled**
   - *Windows Authentication* → **Enabled**
5. F5.

La route di default è `Account/LogOnWithUtenzaTest`. Se `UtenzaTest` è valorizzata, l'app simula
quella matricola (utile in sviluppo).

## 6. Deploy in IIS

### 6.1 Preparazione server

1. Installa **.NET Framework** e il ruolo **IIS** con ASP.NET 4.
2. Installa **Windows Authentication**:
   *Pannello di controllo → Programmi → Attivazione funzionalità Windows → IIS → Servizi Web →
   Sicurezza → Autenticazione Windows*.
3. In *Gestione IIS* → sito → **Autenticazione**:
   - Abilita *Autenticazione Windows*.
   - Disabilita *Autenticazione anonima*.

### 6.2 Pubblicazione

Da Visual Studio: *Build → Pubblica* (profilo File System) oppure:

```powershell
msbuild Source\FCIWeb.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=<profilo>
```

Copia l'output nella cartella del sito IIS e imposta il pool su **.NET CLR v4.0** / *Integrated*.

### 6.3 Directory e permessi

| Percorso | Permesso |
|----------|----------|
| cartella sito | lettura per `IIS_IUSRS` |
| cartella upload/allegati | scrittura per l'identità del pool |
| cartella log | scrittura per l'identità del pool |

## 7. Build

### 7.1 Visual Studio

Semplicemente *Build Solution* (F6). Configurazioni: `Debug` e `Release`.

### 7.2 Riga di comando (Windows)

```powershell
nuget restore FCIWeb.sln
msbuild FCIWeb.sln /p:Configuration=Release /p:Platform="Any CPU"
```

### 7.3 CI

Il workflow [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) esegue validazione statica di
configurazione e documentazione. La build .NET Framework completa richiede un runner Windows; lo
stato è tracciato in `PLAN.md`.

## 8. Test

Al momento non esiste una suite di test nel repository. Passi previsti (fase F5 di `PLAN.md`):

1. progetto di test per la logica pura (`DateUtil`, `Extensions`, `MapObj`, `ModelStateErrors`);
2. comando univoco documentato (`dotnet test` o `nunit3-console`);
3. salvataggio dei log come evidenza in CI.

## 9. Troubleshooting

| Sintomo | Causa probabile | Soluzione |
|---------|-----------------|-----------|
| HTTP 401.2 all'avvio | Anonymous abilitata o Windows disabilitata | Abilita Windows Auth, disabilita Anonymous |
| `A network-related or instance-specific error` (SQL) | connection string errata / SQL non raggiungibile | Verifica `connection.config` e firewall |
| `Login failed for user` | credenziali errate o utente senza permessi | Usa `Integrated Security` o correggi le credenziali |
| `Could not load file or assembly 'AreaCommonWeb'` | DLL mancante in `lib/` o non copiata in `bin` | Fornisci la DLL e imposta *Copy Local = true* |
| `Could not load ... Kendo.Mvc` | versione Kendo errata | Usa la versione referenziata nel `.csproj` |
| `The type initializer for 'FCIContext' threw` | connection string assente | Crea `connection.config` |
| ELMAH non registra errori | connection string `elmah-sql` non definita | Aggiungi `elmah-sql` in `connection.config` (bug noto) |
| Email non inviate | `ServerSmtp` errato / firewall | Verifica SMTP e credenziali |
| Documenti Word non generati | Office non installato (Interop) | Installa Office o migra a OpenXML |
| `Telerik.Web.Mvc` scaduto | licenza Telerik non valida | Installa una licenza valida |

## 10. FAQ

**L'app parte senza database?**
No. `FCIContext` non genera lo schema e non ha inizializzatore: senza DB l'avvio delle pagine dati
fallisce.

**Posso eseguirla su Linux/Docker?**
Non con il target attuale (`System.Web` + .NET Framework). È possibile tentare una build in container
Windows (`mcr.microsoft.com/dotnet/framework/sdk`) o con Mono, ma non è un percorso supportato. Vedi
`PLAN.md` fase F4.

**Perché il repo pubblico non contiene le DLL in `lib/`?**
Perché sono proprietarie/commerciali e non ridistribuibili. Vedi
[`THIRD-PARTY-NOTICES.md`](../THIRD-PARTY-NOTICES.md).

**Come gestisco i segreti?**
Con file `*.sample` + `.gitignore`; in produzione usa variabili d'ambiente o un secret store.
Se hai clonato una versione con credenziali, considerale compromesse e ruotale (`SECURITY.md`).

**Il codice è MIT?**
Il codice originale sì ([`LICENSE`](../LICENSE)); le dipendenze no.

## 11. Glossario

| Termine | Significato |
|---------|-------------|
| **NDG** | Numero di posizione/negozio, identificativo logico della richiesta |
| **UO / UOCode** | Unità organizzativa |
| **Foglio / Riga / Colonna** | Modello dati tabellare configurabile |
| **Wrapper** | Strato/credenziali di accesso ai servizi (`Prod`/`Svi`) |
| **RazorGenerator** | Precompilazione delle viste Razor in `.generated.cs` |
| **ELMAH** | Error Logging Modules and Handlers (log errori) |
| **WSE 3.0** | Web Services Enhancements, stack SOAP legacy |
