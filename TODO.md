# TODO — M7 export CSV sicuro per richieste

## Obiettivo
Aggiungere export CSV autorizzato per l'elenco richieste, riusando filtri, escaping CSV e mitigazione formula injection, con test unitari e HTTP.

## Metrica + soglia
**Metrica**: casi export passati / casi totali; soglia 100%, suite totale verde e build Release `/warnaserror` senza warning.

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
- [x] Docker Compose build/up; login HTTP 200, anonymous export 302 a login.
- [x] Aggiornare PLAN/docs/CodeGraph e secret gates.
- [x] Commit locale Conventional Commit, nessun push.
