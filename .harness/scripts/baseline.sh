#!/usr/bin/env bash
# Captura o estado ATUAL da suíte de testes antes de qualquer história começar.
# Isso vira a referência de "não piorou" — essencial em brownfield, onde já pode
# haver testes falhando ou flaky que não são culpa da história atual.
#
# Uso: ./scripts/baseline.sh

set -euo pipefail
cd "$(dirname "$0")/.."

OUT="stories/.baseline.md"

echo "Rodando suíte de testes completa para capturar baseline..."
RESULT=$(docker exec dotnet-story-sandbox bash -lc \
  "dotnet test --nologo --logger 'console;verbosity=normal'" 2>&1 || true)

TOTAL=$(echo "$RESULT" | grep -oE "Total:\s*[0-9]+" | grep -oE "[0-9]+" || echo "?")
PASSED=$(echo "$RESULT" | grep -oE "Passed:\s*[0-9]+" | grep -oE "[0-9]+" || echo "?")
FAILED=$(echo "$RESULT" | grep -oE "Failed:\s*[0-9]+" | grep -oE "[0-9]+" || echo "0")
SKIPPED=$(echo "$RESULT" | grep -oE "Skipped:\s*[0-9]+" | grep -oE "[0-9]+" || echo "0")

FAILING_NAMES=$(echo "$RESULT" | grep -E "^\s*Failed " | sed 's/^\s*Failed //' || true)

cat > "$OUT" << EOF
# Baseline de testes (capturado antes de qualquer história nova)

Data: $(date -u +"%Y-%m-%d %H:%M UTC")

- Total: ${TOTAL}
- Passando: ${PASSED}
- Falhando: ${FAILED}
- Ignorados/Skip: ${SKIPPED}

## Testes já falhando ANTES desta história (não são responsabilidade dela)
${FAILING_NAMES:-Nenhum}

## Regra de uso
Qualquer história desenvolvida com este harness NÃO pode:
- Reduzir o número de testes passando abaixo do valor acima
- "Corrigir" os testes já falhando listados acima, a menos que seja o objetivo
  explícito da história (nesse caso, seria uma história separada de dívida técnica)

Se ao final de uma história o número de "Passando" for menor que o registrado aqui,
isso é uma regressão e bloqueia o Definition of Done, independente do que a história
pedia.
EOF

echo "Baseline salva em $OUT"
cat "$OUT"
