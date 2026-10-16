#!/usr/bin/env bash
# Validazione XML dei file di configurazione (senza dipendenze .NET).
# Uso: bash scripts/validate-config.sh
set -euo pipefail
cd "$(dirname "$0")/.."

shopt -s nullglob
files=(Source/*.config Source/*.config.sample)
# deduplica
declare -A seen=()
found=0
for f in "${files[@]}"; do
  [ -f "$f" ] || continue
  [ -n "${seen[$f]:-}" ] && continue
  seen[$f]=1
  python3 -c "import xml.dom.minidom,sys; xml.dom.minidom.parse(sys.argv[1])" "$f"
  echo "XML OK: $f"
  found=1
done
[ "$found" = 1 ] || { echo "Nessun file di configurazione trovato." >&2; exit 1; }
echo "Validazione completata."
