# Security Policy — FCIWeb

## Segnalazioni

Per vulnerabilità di sicurezza **non** aprire una issue pubblica. Contatta l'owner del repository
(`Donato Pepe <[PRIVATE_EMAIL]>`) con:

- descrizione della vulnerabilità e impatto;
- passi di riproduzione;
- versione/commit interessato;
- eventuale proof-of-concept.

## Gestione dei segreti

Questo repository è pensato per essere **pubblico** e non deve contenere segreti.

- ❌ Mai committare file di configurazione reali: `Source/connection.config`, `Source/Web.config` con
  credenziali, profili di pubblicazione, `.pubxml`, certificati, chiavi.
- ✅ Usa i template `*.sample` e valorizza i file reali in locale (sono esclusi da `.gitignore`).
- ✅ Fornisci le credenziali via variabili d'ambiente o secret store del CI/CD, non nel codice.

### File sensibili noti

| File | Contenuto sensibile | Stato |
|------|---------------------|-------|
| `Source/connection.config` | Utente/password SQL Server | Escluso e sanificato (vedi `connection.config.sample`) |
| `Source/Web.config` | `Prod/Svi.WrapperBaseUserPassword`, `ServerSmtp`, `UtenzaTest` | Sanificato; valori reali in `Web.config.sample` locale |
| `Source/FCIWeb.csproj` | `PublishDatabaseSettings` con credenziali | Da ripulire |
| `Source/Web.Release.config`, `Web.Debug.config` | connection string di esempio | Verificare |

## Credenziali compromesse

Le credenziali presenti nella history del repository **devono essere considerate compromesse**:

1. **Ruotare** tutte le credenziali coinvolte (SQL Server, wrapper `***REMOVED***`, SMTP).
2. **Riscrivere la history** prima di pubblicare (`git filter-repo` o BFG Repo-Cleaner) per rimuovere
   i segreti da tutti i commit, con backup preventivo e conferma dell'owner.
3. Invalidare eventuali token/chiavi esposti.

### Comando di verifica (working tree)

```bash
grep -rInE "password=|pwd=|user id=|Initial Catalog|Data Source|apikey|api_key|secret|token" \
  --include='*.cs' --include='*.config' --include='*.csproj' --include='*.cshtml' \
  Source old lib 2>/dev/null | grep -viE 'sample|example|placeholder'
```

L'output deve essere vuoto (o contenere solo placeholder).

## Hardening consigliato (debito noto)

- `JsonModelBinder` legge l'intero body senza limite di dimensione → limitare `maxRequestLength`/
  `maxJsonLength` e validare l'input.
- `contentType.Contains(...)` senza null-check in `JsonModelBinder`.
- `catch { }` silenziosi che possono nascondere errori di sicurezza/dati.
- ELMAH configurato con `allowedUsers="*"` → restringere in produzione.
- Autenticazione windows-based: garantire che anonymous sia disabilitata su IIS.

## Versioni supportate

Progetto legacy non mantenuto attivamente: le correzioni di sicurezza sono applicate *best effort*
sul branch `master`.
