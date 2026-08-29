#!/usr/bin/env bash
# Adapta o Aider (rodando com um modelo Ollama local) ao formato que o
# orchestrator.sh espera: um comando que recebe o prompt como argumento único.
#
# Uso (chamado automaticamente pelo orchestrator, não à mão):
#   ./scripts/ollama-agent.sh <modelo> "<prompt>"
#
# Configuração no orchestrator:
#   AGENT_CMD="./scripts/ollama-agent.sh qwen2.5-coder:14b" ./scripts/orchestrator.sh stories/x.md

set -euo pipefail
cd "$(dirname "$0")/.."

MODEL="$1"
PROMPT="$2"

# Aider trabalha diretamente no repositório do host (a pasta ../src, já que
# .harness/ fica ao lado). --yes-always evita prompts interativos de confirmação.
# --no-auto-commits porque o processo do harness já controla os commits via
# instrução no próprio AGENT.md (o agente decide quando commitar).
aider \
  --model "ollama/${MODEL}" \
  --yes-always \
  --no-auto-commits \
  --message "$PROMPT" \
  ../src
