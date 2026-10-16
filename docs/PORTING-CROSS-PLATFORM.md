# Porting cross-platform — FCIWeb

Obiettivo: rendere FCIWeb **eseguibile e verificabile su Linux/macOS/Windows** senza dipendenze
proprietarie o commerciali, preservando il comportamento del dominio.

Lo stato e la pianificazione sono tracciati in [`PLAN.md`](../PLAN.md).

---

## 1. Perché serve un porting

Il progetto originale è ASP.NET MVC 4 su **.NET Framework** e dipende da:

| Dipendenza legacy | Problema cross-platform |
|-------------------|-------------------------|
| `System.Web` / `System.Web.Mvc` | non esistono in .NET moderno (sono Windows-only) |
| `Global.asax`, `Web.config` | modello di hosting IIS, non portabile |
| Entity Framework 5 | sostituibile con EF Core |
| Telerik MVC + Kendo UI for MVC | commerciali, helper server-side |
| `AreaCommonWeb.dll`, `WebServiceModel.dll` | proprietarie, non ridistribuibili |
| `Microsoft.Office.Interop.Word` | richiede Windows + Word |
| `Microsoft.Web.Services3` (WSE) | stack SOAP obsoleto |
| RazorGenerator (viste precompilate `*.generated.cs`) | sostituibile con Razor a runtime |
| `System.Drawing` | supportato ma sconsigliato fuori Windows |

Un semplice "build con Mono" non basta: Mono non esegue in modo affidabile ASP.NET MVC 4/System.Web
e comunque non eliminerebbe le dipendenze proprietarie. La strada corretta è il **porting a .NET 8**.

## 2. Strategia

Approccio **strangler**: il legacy resta in `Source/` come riferimento, il nuovo codice cresce in
parallelo in `src/` e sostituisce progressivamente il legacy. Ogni passo è verificato da test
eseguibili su Linux.

```
FCIWeb.sln                     (legacy .NET Framework — riferimento)
FCIWeb.CrossPlatform.sln       (nuovo .NET 8 — target)
  src/FCIWeb.Core              logica di dominio portabile
  tests/FCIWeb.Core.Tests      test cross-platform (dotnet test)
```

## 3. Mappa di conversione

| Legacy | Cross-platform | Note |
|--------|----------------|------|
| `System.Web.Mvc` Controller/ViewResult | `Microsoft.AspNetCore.Mvc` | `ActionResult` → `IActionResult` |
| `Global.asax` + `App_Start` | `Program.cs` + `Startup`/minimal hosting | |
| `Web.config` connection string | `appsettings.json` + `IConfiguration` | niente segreti nel repo |
| `System.Configuration.ConfigurationManager.AppSettings` | `IConfiguration` / `IOptions<T>` | |
| Entity Framework 5 (`DbContext`) | EF Core (`DbContext` + provider) | SQL Server provider cross-platform; SQLite per demo |
| `System.Web.HttpPostedFileBase` | `IFormFile` | upload |
| `HttpContext.Current` | `IHttpContextAccessor` | |
| `System.Web.Security` / Windows auth | Cookie auth per demo, `Negotiate` opzionale su Windows | |
| Razor views + RazorGenerator | Razor a runtime (ASP.NET Core) | eliminare `*.generated.cs` |
| Telerik MVC / Kendo MVC helpers | Kendo UI for jQuery lato client (o alternativa permissiva) | decisione in F7 |
| `Microsoft.Office.Interop.Word` | `DocumentFormat.OpenXml` (già referenziato) o `OpenXml`/`QuestPDF` | |
| `Microsoft.Web.Services3` (WSE) | `HttpClient` + `System.ServiceModel` / REST | |
| `AreaCommonWeb.dll`, `WebServiceModel.dll` | interfacce + adapter (mock in `DEMO_MODE`) | rimuove le dipendenze proprietarie |
| `System.Drawing` (TabelleDB) | `SkiaSharp`/`ImageSharp` oppure rimozione | solo se usato |

## 4. Fasi

> `IResourcesLookup` sostituisce i repository proprietari istanziati dentro i modelli legacy:
> l'accesso a utenti/UO è ora un'astrazione (`src/FCIWeb.Core/Services`), facilmente sostituibile in
> `DEMO_MODE` e implementabile su EF Core in M3.

| Fase | Contenuto | Stato |
|------|-----------|-------|
| M0 | Scaffold soluzione .NET 8 + CI cross-platform | ✅ |
| M1 | Porting utility pure (`DateUtil`, `Extensions`, `MapObj`) + test | ✅ (26 test) |
| M2 | Porting modelli di dominio (entità) senza `System.Web`/`System.Drawing` | ✅ (42 test) |
| M3 | EF Core context + migrazioni + provider SQLite (demo) e SQL Server | ⏳ |
| M4 | Web app ASP.NET Core: hosting, routing, auth cookie/Windows, `DEMO_MODE` | ✅ (105 test, container Linux) |
| M5 | Porting repository/servizi e adapter per i servizi esterni | ⏳ |
| M6 | Porting viste Razor a runtime + UI (sostituzione Telerik/Kendo) | ⏳ |
| M7 | Documenti: sostituzione Office Interop con OpenXml | ⏳ |
| M8 | Pacchettizzazione Docker (`linux/amd64`, `linux/arm64`) + release | ⏳ |

## 5. Definizione di "fatto" per il cross-platform

- `dotnet build` e `dotnet test` verdi su Linux (evidenza in CI).
- App avviabile in container Linux in `DEMO_MODE` **senza** SQL Server, senza web service interni e
  senza dipendenze commerciali.
- Nessuna dipendenza proprietaria/commerciale nel percorso cross-platform.
- Documentazione aggiornata e comandi riproducibili.

## 6. Vincoli

- Il legacy `Source/` non viene cancellato: resta come riferimento finché il porting non è completo.
- Ogni passo preserva il comportamento documentato dai test; le differenze intenzionali sono
  annotate in questo file.
