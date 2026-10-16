# Contributing — FCIWeb

Grazie per l'interesse. Questo è un progetto legacy: le modifiche devono restare **piccole,
tracciabili e verificate**.

## Prima di aprire una PR

1. Leggi [`README.md`](README.md), [`PLAN.md`](PLAN.md) e [`docs/ANALISI-CODICE.md`](docs/ANALISI-CODICE.md).
2. Verifica che la modifica risolva un item del backlog in `PLAN.md` (o apri prima una issue).
3. **Nessun segreto**: usa i file `*.sample`, mai credenziali reali.
4. **Nessuna DLL proprietaria**: non aggiungere binari in `lib/` o asset commerciali.
5. Aggiorna la documentazione e `PLAN.md` (registro cicli) insieme al codice.

## Flusso

```bash
git checkout -b fix/<breve-descrizione>
# ... modifica ...
git add -A
git commit -m "fix: <descrizione> (PLAN §<item>)"
```

## Convenzioni di commit

Formato [Conventional Commits](https://www.conventionalcommits.org/):

- `fix:` correzione di bug
- `feat:` nuova funzionalità
- `docs:` documentazione
- `chore:` manutenzione, build, CI
- `test:` test
- `refactor:` ristrutturazione senza cambi funzionali
- `security:` correzione di sicurezza

## Requisiti minimi per il merge

- [ ] La modifica è una sola e ha un criterio di done misurabile.
- [ ] Nessun segreto aggiunto (`SECURITY.md`).
- [ ] Nessuna dipendenza non ridistribuibile aggiunta.
- [ ] Documentazione aggiornata se cambia il comportamento.
- [ ] Evidenza allegata (log/schermata) per comportamento dichiarato.

## Stile del codice

- C#: segui lo stile esistente; `.editorconfig` presente nella root.
- Evita `catch { }` vuoti: logga o gestisci.
- Preferisci nomi espliciti; commenta solo il *perché*, non il *cosa*.

## Licenza dei contributi

Contribuendo accetti che il tuo contributo sia distribuito sotto la licenza
[MIT](LICENSE) per la parte originale. Le dipendenze di terze parti restano soggette alle proprie
licenze ([`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md)).
