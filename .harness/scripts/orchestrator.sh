#!/usr/bin/env bash
# Loop orquestrador agnóstico de vendor.
# Funciona com qualquer agente que aceite um prompt não-interativo via CLI
# (Claude Code CLI, aider, ou outro) — configure em AGENT_CMD.
#
# Uso:
#   AGENT_CMD="claude -p" ./scripts/orchestrator.sh stories/minha-historia.md
#   AGENT_CMD="aider --message" ./scripts/orchestrator.sh stories/minha-historia.md
#
# Se o agente já suporta MCP nativamente (caso do Claude Code, apontando pro
# .mcp.json deste projeto), ele mesmo vai chamar dotnet_build/dotnet_test via MCP
# a cada iteração. Este script é a camada de segurança adicional: valida de forma
# independente (direto no container, sem depender do agente ter chamado a tool
# certa) se a história realmente está pronta, e decide se pede mais uma rodada.

set -euo pipefail
cd "$(dirname "$0")/.."

STORY_FILE="${1:?Uso: orchestrator.sh <arquivo-da-historia>}"
SLUG=$(basename "$STORY_FILE" .md)
STATE_FILE="stories/.state-${SLUG}.md"
MAX_ITER="${MAX_ITER:-15}"
AGENT_CMD="${AGENT_CMD:?Defina AGENT_CMD, ex: AGENT_CMD=\"claude -p\"}"

run_in_sandbox() {
  docker exec dotnet-story-sandbox bash -lc "$1"
}

definition_of_done_ok() {
  echo "Verificando Definition of Done (independente do agente)..."
  run_in_sandbox "dotnet build -warnaserror --nologo" || return 1
  run_in_sandbox "dotnet test --nologo" || return 1
  run_in_sandbox "dotnet format --verify-no-changes" || return 1
  return 0
}

build_prompt() {
  local iter="$1"
  cat <<EOF
Leia AGENT.md e siga o processo descrito lá para a história em ${STORY_FILE}.

Iteração ${iter}/${MAX_ITER}. Se existir estado em ${STATE_FILE}, retome de lá em vez de
recomeçar do zero. Ao final desta iteração, atualize o estado com o progresso feito.

Se todos os critérios de aceite já estiverem cobertos por teste e passando, apenas
confirme isso no estado e não faça mais alterações.
EOF
}

echo "== Desenvolvendo história: ${STORY_FILE} =="

for i in $(seq 1 "$MAX_ITER"); do
  echo ""
  echo "--- Iteração $i/$MAX_ITER ---"

  PROMPT=$(build_prompt "$i")
  $AGENT_CMD "$PROMPT"

  if definition_of_done_ok; then
    echo ""
    echo "Definition of Done atingido na iteração $i."
    exit 0
  fi

  echo "Ainda não está pronto — próxima iteração."
done

echo ""
echo "Limite de $MAX_ITER iterações atingido sem fechar o Definition of Done."
echo "Verifique ${STATE_FILE} para ver onde travou."
exit 1
