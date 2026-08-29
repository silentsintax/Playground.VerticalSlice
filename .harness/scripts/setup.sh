#!/usr/bin/env bash
# Sobe o sandbox .NET no Docker Desktop e restaura os pacotes.
# Uso: ./scripts/setup.sh

set -euo pipefail
cd "$(dirname "$0")/.."

if ! docker info >/dev/null 2>&1; then
  echo "Docker Desktop não parece estar rodando. Abra o Docker Desktop e tente de novo."
  exit 1
fi

echo "Subindo container sandbox..."
docker compose up -d

echo "Aguardando container ficar pronto..."
sleep 2

echo "Restaurando pacotes NuGet dentro do container..."
docker exec dotnet-story-sandbox bash -lc "dotnet restore"

echo ""
echo "Sandbox pronto. Container: dotnet-story-sandbox"
echo "Para parar depois: docker compose down"
