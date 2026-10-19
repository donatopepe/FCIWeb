# Security Policy — FCIWeb

## Segnalazioni

Per vulnerabilità di sicurezza **non** aprire una issue pubblica. Contatta l'owner del repository
(`Donato Pepe <contact@example.invalid>`) con:

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

1. **Ruotare** tutte le credenziali coinvolte (SQL Server, wrapper `Prod`/`Svi`, SMTP).
2. **Riscrivere la history** (`git filter-repo` o BFG) per rimuovere i segreti da tutti i commit.
3. Invalidare eventuali token/chiavi esposti.

## Stato attuale della history (2026-10-16)

- History riscritta con `git filter-repo` (2 passate) e **verificata**: 0 occorrenze dei segreti noti
  su tutti i commit.
- Il repository GitHub è stato **cancellato e ricreato** per eliminare gli oggetti orfani con segreti.
- Gate disponibili:
  - `bash scripts/check-secrets.sh` → working tree
  - `bash scripts/check-history-secrets.sh` → storia git, tramite deny-list locale non versionata
    (`.secrets-denylist`, escluso da `.gitignore`; oppure `FCIWEB_DENYLIST=/path/file`).
- La deny-list **non è versionata di proposito**: elencare i valori li ripubblicherebbe.
- Backup locali (fuori dal repo): `/root/fciweb-backup-20261016/`, `/root/fciweb-secrets-backup/`,
  `/root/fciweb-remote-backup.git`.

### Comando di verifica (working tree)

```bash
grep -rInE "password=|pwd=|user id=|Initial Catalog|Data Source|apikey|api_key|secret|token" \
  --include='*.cs' --include='*.config' --include='*.csproj' --include='*.cshtml' \
  Source old lib 2>/dev/null | grep -viE 'sample|example|placeholder'
```

L'output deve essere vuoto (o contenere solo placeholder).

## Controlli implementati (stack cross-platform .NET 8)

Login applica inoltre lockout progressivo per coppia username/IP: default 5 errori in 10 minuti, lockout
15 minuti. Lo stato è in memoria del processo (single instance); deployment multi-instance richiede
rate limiting distribuito/reverse-proxy e validazione configurazione.

| Controllo | Dove | Evidenza |
|-----------|------|----------|
| Validazione content-type null-safe | `SecurityGuards.IsJsonContentType` | test unitari |
| Limite dimensione payload | `SecurityGuards.EnsureWithinSize` (1 MiB default) | test unitari |
| Sanitizzazione nomi file (traversal Windows+POSIX) | `SecurityGuards.SanitizeFileName` | test unitari |
| Difesa path traversal (drive/UNC/POSIX) | `SecurityGuards.IsSafeRelativePath`/`CombineUnderRoot` | test unitari |
| Mascheramento segreti nei log | `SecretRedactor` | test unitari |
| Validazione configurazione SMTP | `SecureSmtpValidator` | test unitari |
| Header CSP/HSTS, anti-frame, nosniff, referrer | `SecurityHeadersExtensions` | test integrazione |
| Login lockout per account+IP | `LoginAbuseGuard` (per-process) | test integrazione con clock controllabile |
| CSV formula injection | `CsvExport.NeutralizeSpreadsheetFormula` | test payload `= + - @` e prefissi whitespace/control |
| CSV authentication/filter | `RichiesteController.ExportCsv` | test auth redirect, content type/attachment, query filtering |
| XLSX formula safety | `XlsxExport` emits all values as inline strings (no formula cells) | tests parse workbook and verify untrusted values remain inline strings |
| XLSX authentication/filter | `RichiesteController.ExportXlsx` | HTTP tests auth redirect, MIME/attachment, query filtering |
| XLSX formula safety | `XlsxExport` emits all values as inline strings (no formula cells) | tests parse workbook and verify untrusted values remain inline strings |
| XLSX authentication/filter | `RichiesteController.ExportXlsx` | HTTP tests auth redirect, MIME/attachment, query filtering |
| ReturnUrl anti-open-redirect | `AccountController.Login` | test URL esterno |
| Cookie Secure/HttpOnly/SameSite | `Program.cs` | test Set-Cookie |
| Antiforgery su tutte le POST | `AutoValidateAntiforgeryTokenAttribute` | test integrazione (400 senza token) |
| Password con PBKDF2-SHA256 (100k iterazioni) | `DemoUserStore` | revisione codice |
| Autorizzazione per ruolo | `RichiesteController.Delete` (`[Authorize(Roles="admin")]`) | verifica empirica (accesso negato) |
| Cookie `HttpOnly`, `SameSite=Lax`, scadenza | `Program.cs` | revisione codice |
| Container non-root + `read_only` + `no-new-privileges` | `Dockerfile`, `docker-compose.yml` | build + run |
| Nessun header `Server` | `Kestrel.AddServerHeader=false` | verifica curl |
| Analizzatori .NET bloccanti | `src/Directory.Build.props`, CI `/warnaserror` | build 0 warning |

## Hardening consigliato (debito noto, legacy `Source/`)

Questi punti riguardano il progetto legacy .NET Framework e sono **già risolti** nel nuovo stack:

- `JsonModelBinder` legge l'intero body senza limite di dimensione → nel nuovo stack il limite è
  centralizzato in `SecurityGuards`.
- `contentType.Contains(...)` senza null-check in `JsonModelBinder` → `IsJsonContentType` è null-safe.
- `catch { }` silenziosi che possono nascondere errori di sicurezza/dati.
- ELMAH configurato con `allowedUsers="*"` → restringere in produzione.
- Autenticazione windows-based: garantire che anonymous sia disabilitata su IIS.

## Versioni supportate

Progetto legacy non mantenuto attivamente: le correzioni di sicurezza sono applicate *best effort*
sul branch `master`.
