# `lib/` — dipendenze non ridistribuibili

Questa cartella **non** è versionata nel repository pubblico perché contiene librerie
proprietarie e commerciali che **non possono essere ridistribuite**.

Il codice sorgente originale di FCIWeb è distribuito sotto licenza [MIT](../LICENSE); le librerie
elencate qui restano soggette alle rispettive licenze (vedi [`THIRD-PARTY-NOTICES.md`](../THIRD-PARTY-NOTICES.md)).

## File attesi (forniti dall'owner / da acquisire separatamente)

| File | Componente | Licenza |
|------|-----------|---------|
| `AreaCommonWeb.dll` | Libreria interna aziendale | Proprietaria |
| `WebServiceModel.dll` | Proxy web service interni | Proprietaria |
| `Telerik.Web.Mvc.dll` | Telerik Extensions for ASP.NET MVC | Commerciale |
| `KENDOUIMVC/<versione>/Kendo.Mvc.dll` | Kendo UI for ASP.NET MVC | Commerciale |
| `itextsharp.dll` | iTextSharp 5 | AGPL-3.0 o commerciale |
| `Microsoft.Office.Interop.Word.dll` | Office Interop | Microsoft EULA |
| `Microsoft.Web.Services3.dll` | WSE 3.0 | Microsoft (legacy) |

## Come ripristinare una build locale

1. Copia le DLL nei percorsi attesi da `Source/FCIWeb.csproj` (`HintPath` → `..\lib\...`).
2. Anche gli asset Telerik/Kendo sono esclusi dal repo pubblico:
   - `Source/Scripts/2012.1.214/`, `Source/Scripts/kendo/`
   - `Source/Content/2012.1.214/`, `Source/Content/kendo/`
3. Esegui `nuget restore FCIWeb.sln` per i pacchetti NuGet.
4. Apri `FCIWeb.sln` in Visual Studio su Windows.

Senza queste dipendenze il progetto **non compila e non funziona**: è una scelta deliberata per
rendere pubblicabile il solo codice originale.
