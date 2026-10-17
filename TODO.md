# TODO — M7 document export cross-platform

## Obiettivo del ciclo
Aggiungere export XLSX cross-platform per richieste usando Open XML SDK (MIT), senza Office Interop, mantenendo export CSV esistente.

## Metrica + soglia
**Metrica**: casi export XLSX superati / casi totali; soglia **100%**, suite completa verde e build Release `/warnaserror` senza warning/errori.

## Casi ordinati (definiti prima dell'implementazione)
1. XLSX non vuoto, apribile da OpenXmlReader, foglio `Richieste`, intestazioni e righe.
2. Celle testuali con escape XML/UTF-8 corretto; NDG e note non interpretati come formule (inline strings).
3. Zero record produce workbook valido con solo header.
4. HTTP endpoint protetto da auth, query filtro rispettata e Content-Disposition `.xlsx`.
5. Nessun dipendente da Office/COM/Windows.

## Dipendenze
- EF/in-memory stores, ASP.NET Core MVC, test xUnit già presenti.
- NuGet `DocumentFormat.OpenXml` (licenza MIT; verificare transitive licenses / THIRD-PARTY-NOTICES).

## Task
- [ ] Aggiungere serializer XLSX con Open XML SDK in `src/FCIWeb.Web` o `src/FCIWeb.Data`.
- [ ] Aggiungere endpoint auth `ExportXlsx`, filtro coerente e nome file.
- [ ] Test XLSX struttura/escaping/empty e test HTTP auth/filter.
- [x] Aggiornare third-party notices, README, guida, porting, PLAN §2/§6/§10.
- [x] Full suite 142/142 e build `/warnaserror` 0/0 nel container; secret/history gates OK.
- [x] Docker Compose build/up/down Linux; login HTTP 200, export XLSX anonimo 302 login.
- [ ] Commit locale conventional, nessun push.


## Casi ordinati, definiti prima dell'implementazione
1. CSV valido: header, UTF-8, righe/campi con virgole, quote e newline correttamente escaped.
2. Input untrusted che inizia `=`, `+`, `-`, `@`, tab o CR/LF viene neutralizzato per spreadsheet formula injection.
3. Query filtro usa lo stesso filtro della lista; risultato scaricabile con Content-Disposition.
4. Non autenticato non può esportare (redirect login); POST antiforgery non coinvolto (endpoint GET read-only).
5. Dataset empty produce header-only CSV.

## Task
- [x] Definire serializer CSV e formula-safety in servizio testabile.
- [x] Aggiungere endpoint autenticato download e propagare query.
- [x] Aggiungere test unitari escaping/formula injection e test MVC headers/auth/filter.
- [x] Verificare full `dotnet test` (138), build `/warnaserror` (0/0).
- [x] Revalidare dopo commit: 138 test e Docker Compose login HTTP 200, anonymous export 302 a login.
- [x] Docker Compose build/up; login HTTP 200, anonymous export 302 a login.
- [x] Aggiornare PLAN/docs/CodeGraph e secret gates.
- [x] Commit locale Conventional Commit, nessun push.
