# TODO — M6 list paging e UI cross-platform

## Obiettivo del ciclo
Aggiungere paging server-side all'elenco Richieste per evitare di materializzare l'intero archivio e rendere la lista utilizzabile con dataset crescenti.

## Metrica + soglia
**Tasso test scenari paging passati** = test paging passati / test paging totali. Soglia **100%**; suite completa verde e build `/warnaserror` senza warning/errori.

## Scenari definiti prima dell'implementazione (ordine)
1. Pagina 1 e pagina 2, ordinamento deterministico e nessun record duplicato.
2. Ricerca testuale: filtro applicato prima di count e paging.
3. Page fuori range/zero e page size invalidi: normalizzazione e limite massimo 50.
4. Elenco vuoto: count 0, page corrente 1, pages 0.

## Dipendenze
- M4 web MVC e M3 repository EF/in-memory presenti.
- Nessuna nuova dipendenza.

## Security hardening ciclo 14
- [x] Lockout login per username+IP (5 failure/10 min, lock 15 min), time provider controllabile.
- [x] Test lockout, sblocco, returnUrl esterno, cookie Secure/HttpOnly/SameSite e HSTS.
- [x] Suite finale ciclo 14: Core 96, Data 15, Web 15 = 126 PASS; build `/warnaserror` 0/0; Docker smoke HTTP 200.
- [ ] Migrare lockout storage a backend distribuito prima di deployment multi-instance.

## Task
- [x] Aggiungere result paging e API su store/repository.
- [x] Implementare query paginata in EF (count + Skip/Take server-side) e store demo.
- [x] Aggiornare controller e view con pager accessibile.
- [x] Aggiungere test repository e web per gli scenari sopra.
- [x] Test suite completa: Core 96, Data 15, Web 12 = 123 PASS.
- [x] Build Release `/warnaserror`: 0 warning/error; Docker Compose up login 200/down OK.
- [x] Aggiornare PLAN/docs, secret gates, commit locale; niente push.
