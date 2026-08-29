#!/usr/bin/env bash
# Cria uma nova história a partir do template.
# Uso: ./scripts/new-story.sh nome-da-historia

set -euo pipefail
cd "$(dirname "$0")/.."

SLUG="${1:?Uso: ./scripts/new-story.sh <slug-da-historia>}"
DEST="stories/${SLUG}.md"

if [[ -f "$DEST" ]]; then
  echo "Já existe: $DEST"
  exit 1
fi

cp stories/_template.md "$DEST"
echo "Criado: $DEST — edite os critérios de aceite e o escopo antes de começar."
