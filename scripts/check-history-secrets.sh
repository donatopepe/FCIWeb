#!/usr/bin/env bash
# Gate locale: verifica che la STORIA git non contenga segreti noti.
#
# La lista dei valori sensibili NON e' versionata (elencarli li ripubblicherebbe).
# Va fornita tramite:
#   - file locale .secrets-denylist (escluso da .gitignore), una stringa per riga; oppure
#   - variabile d'ambiente FCIWEB_DENYLIST=/path/to/file.
#
# Uso: bash scripts/check-history-secrets.sh
set -euo pipefail
cd "$(dirname "$0")/.."

DENYLIST="${FCIWEB_DENYLIST:-.secrets-denylist}"

if [ ! -f "$DENYLIST" ]; then
  echo "ATTENZIONE: deny-list '$DENYLIST' assente: salto il controllo della storia." >&2
  echo "            Vedi SECURITY.md per crearla." >&2
  exit 0
fi

hits=0
while IFS= read -r pattern; do
  [ -z "$pattern" ] && continue
  case "$pattern" in \#*) continue ;; esac
  n=$(git grep -I -c -- "$pattern" $(git rev-list --all) 2>/dev/null | wc -l || true)
  if [ "$n" -gt 0 ]; then
    echo "TROVATO nella storia: '$pattern' ($n blob)" >&2
    hits=$((hits+n))
  fi
done < "$DENYLIST"

if [ "$hits" -gt 0 ]; then
  echo "ERRORE: $hits occorrenze di segreti noti nella storia git. Vedi SECURITY.md." >&2
  exit 1
fi
echo "OK: nessun segreto noto nella storia git."
