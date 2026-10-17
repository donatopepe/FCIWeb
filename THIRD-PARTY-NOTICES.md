# Third-Party Notices — FCIWeb

Questo documento elenca i componenti di terze parti utilizzati da FCIWeb che **non** ricadono sotto
la licenza MIT del codice originale. Le voci contrassegnate con ❌ **non sono ridistribuibili** senza
i relativi diritti/licenze: le DLL corrispondenti non devono essere pubblicate in un repository
pubblico.

> Stato: analisi statica dei riferimenti in `Source/FCIWeb.csproj`, `Source/packages.config` e `lib/`.
> Le versioni commerciali e le condizioni vanno confermate dall'owner prima di qualunque pubblicazione.

## 1. NuGet usati dal porting .NET 8

| Pacchetto | Versione | Licenza | Uso |
|-----------|----------|---------|-----|
| DocumentFormat.OpenXml | 3.3.0 | MIT (NuGet metadata) | Generazione XLSX cross-platform, senza Office/Interop |

Verificare l'elenco aggiornato delle dipendenze transitive al restore tramite `dotnet list package --include-transitive`.
DocumentFormat.OpenXml e il suo framework OpenXml sono MIT; l'inventario transitive corrente include i pacchetti già presenti via EF Core/design, da valutare secondo licenze individuali prima della release. Nessuna dipendenza commerciale è stata aggiunta dal porting XLSX.

## 2. Librerie non NuGet in `lib/` — ❌ bloccanti la pubblicazione

| Componente | File | Licenza / natura | Ridistribuibile | Azione |
|-----------|------|------------------|-----------------|--------|
| AreaCommonWeb | `lib/AreaCommonWeb.dll` | Proprietaria (libreria interna aziendale) | ❌ | Escludere dal repo pubblico; documentare come prerequisito |
| WebServiceModel | `lib/WebServiceModel.dll` | Proprietaria (proxy web service interni) | ❌ | Escludere dal repo pubblico |
| Telerik Extensions for ASP.NET MVC | `lib/Telerik.Web.Mvc.dll` | Commerciale (Telerik, licenza per sviluppatore) | ❌ | Rimuovere; richiedere licenza Telerik |
| Kendo UI for ASP.NET MVC | `lib/KENDOUIMVC/2013.2.716.340`, `2013.2.918.340`, `2015.2.902` | Commerciale (Progress/Telerik) | ❌ | Rimuovere; richiedere licenza Kendo UI |
| iTextSharp | `lib/itextsharp.dll` | AGPL-3.0 oppure licenza commerciale | ⚠️ | Sostituire con alternativa permissiva (es. PdfSharp/QuestPDF) o mantenere AGPL |
| Microsoft Office Interop Word | `lib/Microsoft.Office.Interop.Word.dll` | Microsoft EULA / richiede Word installato | ⚠️ | Valutare sostituzione con `DocumentFormat.OpenXml` (già presente) |
| Microsoft Web Services Enhancements 3.0 | `lib/Microsoft.Web.Services3.dll` | Legacy Microsoft (WSE 3.0) | ⚠️ | Deprecato: valutare rimozione |

## 2. Pacchetti NuGet (`Source/packages.config`)

Le licenze dei pacchetti NuGet sono soggette ai rispettivi termini. Voci principali:

| Pacchetto | Versione | Licenza tipica |
|-----------|----------|----------------|
| Microsoft.AspNet.Mvc | 4.0.20710.0 | MS-EULA (free) |
| Microsoft.AspNet.Razor | 2.0.30506.0 | MS-EULA (free) |
| Microsoft.AspNet.WebPages (+Data/OAuth/WebData) | 2.0.30506.0 / 2.0.20710.0 | MS-EULA (free) |
| Microsoft.AspNet.Web.Optimization | 1.0.0 | MS-EULA (free) |
| Microsoft.AspNet.Mvc.FixedDisplayModes | 1.0.0 | MS-EULA (free) |
| EntityFramework | 5.0.0 | Apache-2.0 |
| ELMAH (`elmah.corelibrary`, `Elmah.MVC`) | 1.2.2 / 2.1.1 | Apache-2.0 |
| DotNetOpenAuth.* | 4.0.4.12182 | MPL-2.0 / Apache-2.0 |
| RazorGenerator.Mvc | 2.2.2 | Apache-2.0 |
| RazorEngine | 3.3.0 | MS-PL |
| Quartz | 2.1.2 | Apache-2.0 |
| NPOI | 2.1.3.1 | Apache-2.0 |
| DocumentFormat.OpenXml | 2.7.1 | MIT |
| ActionMailer | 0.7.4 | MIT |
| jQuery | 1.8.3 | MIT |
| jQuery.UI.Combined | 1.10.4 | MIT |
| jQuery.Validation | 1.10.0 | MIT |
| Microsoft.jQuery.Unobtrusive.Ajax / Validation | 3.1.1 / 2.0.30116.0 | Apache-2.0 |
| Modernizr | 1.7 | MIT/BSD |
| WebGrease | 1.1.0 | Apache-2.0 |
| WebActivator / WebActivatorEx | 1.2.0.0 / 2.0.5 | MS-PL / MIT |
| Common.Logging | 2.1.2 | Apache-2.0 |
| CodeContracts.Unofficial | 1.0.0.2 | MS-PL |
| DataAnnotationsExtensions(+MVC3) | 1.1.0.0 | BSD |
| Microsoft.Web.Infrastructure | 1.0.0.0 | MS-EULA (free) |
| UpgradeMvc3ToMvc4 | 1.0.1 | MS-EULA (free) |

> Le licenze indicate sono quelle comunemente pubblicate per le rispettive versioni; fare sempre
> riferimento al pacchetto NuGet originale e al file `licenseUrl`/`LICENSE` incluso.

## 3. Asset front-end vendorizzati in `Source/Scripts` e `Source/Content`

Questi sono interi progetti di terze parti copiati nel repository e **non** sono coperti dalla MIT:

| Cartella / file | Progetto | Licenza |
|-----------------|----------|---------|
| `Source/Scripts/ce-master`, `ce-master.zip` | ce (spreadsheet editor) | vedi `ce-master/LICENSE` |
| `Source/Scripts/x-spreadsheet-master` | x-spreadsheet | MIT |
| `Source/Scripts/kendo`, `Source/Content/kendo` | Kendo UI | Commerciale ❌ |
| `Source/Scripts/2012.1.214`, `Source/Content/2012.1.214` | Telerik MVC assets | Commerciale ❌ |
| `Source/Scripts/jquery*`, `jquery-ui*`, `jquery.validate*` | jQuery Foundation | MIT |
| `Source/Content/MaterialIcons.*` | Google Material Icons | Apache-2.0 |
| `Source/Scripts/modernizr-1.7*` | Modernizr | MIT/BSD |
| `Source/Scripts/knockout-3.5.1.js` | Knockout | MIT |
| `Source/Scripts/DataTables`, `Dialog`, `ContextMenu`, `jexcel_custom` | varie | da verificare singolarmente |

## 4. Documenti in `Source/Doc_Files`

I file `.docx` contengono modulistica aziendale e vanno valutati dall'owner prima della pubblicazione
(potenziale contenuto riservato).

## 5. Azioni richieste prima della pubblicazione

1. Rimuovere `lib/` (o spostarlo in un archivio privato) e documentarlo come prerequisito esterno.
2. Rimuovere/valutare gli asset commerciali Telerik/Kendo sotto `Scripts` e `Content`.
3. Verificare e completare questa tabella con le licenze esatte dei progetti vendorizzati.
4. Verificare il contenuto di `Source/Doc_Files` e di `Da aggiornare sul server.7z`.
5. Aggiornare questo file a ogni aggiunta/rimozione di dipendenza.
