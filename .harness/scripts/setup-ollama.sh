#!/usr/bin/env bash
# Prepara Ollama + Aider para rodar o harness 100% local, sem nenhuma API paga.
# Uso: ./scripts/setup-ollama.sh [modelo]
# Ex:  ./scripts/setup-ollama.sh qwen2.5-coder:14b

set -euo pipefail
MODEL="${1:-qwen2.5-coder:14b}"

if ! command -v ollama &> /dev/null; then
  echo "Ollama não encontrado. Instale em https://ollama.com/download antes de continuar."
  exit 1
fi

echo "Verificando se o Ollama está rodando..."
if ! curl -s http://localhost:11434/api/tags >/dev/null; then
  echo "Ollama não parece estar rodando. Inicie com 'ollama serve' (ou abra o app) e tente de novo."
  exit 1
fi

echo "Baixando modelo ${MODEL} (pode demorar bastante na primeira vez)..."
ollama pull "$MODEL"

if ! command -v aider &> /dev/null; then
  echo "Instalando Aider..."
  pip install aider-chat --break-system-packages 2>/dev/null || pip install aider-chat
fi

echo ""
echo "Setup concluído. Modelo pronto: ${MODEL}"
echo "Para usar no orchestrator:"
echo ""
echo "  AGENT_CMD=\"./scripts/ollama-agent.sh ${MODEL}\" ./scripts/orchestrator.sh stories/sua-historia.md"
