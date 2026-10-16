#!/usr/bin/env bash
# Gate locale: verifica che il working tree non contenga credenziali evidenti.
# Uso: bash scripts/check-secrets.sh
set -euo pipefail
cd "$(dirname "$0")/.."

if grep -rInE "Password=[^\"'<]{3,}|pwd[0-9]{3,}|User ID=[A-Za-z0-9_]+;.*Password=" \
     --include='*.config' --include='*.cs' --include='*.csproj' --include='*.cshtml' \
     --include='*.json' --include='*.yml' --include='*.yaml' \
     Source lib 2>/dev/null \
     | grep -vE '\.sample|__SET_ME__|<DB_|placeholder' ; then
  echo "ERRORE: possibili segreti trovati. Vedi SECURITY.md." >&2
  exit 1
fi
echo "OK: nessun segreto evidente nel working tree."
