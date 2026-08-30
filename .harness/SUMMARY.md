# Harness .NET — Resumo Final

## Objetivo
Permitir que agentes de IA (Claude, GitHub Copilot, Devin, ou modelos locais via
Ollama) desenvolvam histórias de usuário .NET do início ao fim — de forma autônoma,
verificável e sem dependência de nenhum vendor específico — rodando 100% local com
Docker Desktop.

## Princípios de design

1. **O harness nunca confia no que o agente diz ter feito.** Toda alteração é
   validada de forma independente (build, test, format) rodando dentro de um
   container isolado — nunca no host, nunca "porque o agente disse que passou".
2. **Memória vive em arquivo, não em contexto de conversa.** Isso permite retomar
   uma história depois de `/compact`, reinício de sessão, ou troca de agente no meio
   do caminho.
3. **Nenhuma lógica depende de um vendor específico.** MCP (protocolo aberto), Docker
   (padrão de mercado) e Node.js puro (sem shell) são as únicas dependências reais.
4. **Trocar de agente ou de modelo é trocar uma flag, nunca reescrever nada.**

---

## Estrutura de arquivos e o porquê de cada um

```
meu_repo_git/
├── .mcp.json                  ← na raiz (Claude Code procura aqui)
├── src/ (ou onde estiver)     ← seu código real, intocado
└── .harness/
    ├── AGENT.md
    ├── ONBOARDING.md
    ├── agents.config.json
    ├── harness.js
    ├── docker-compose.yml
    ├── mcp-server/
    ├── scripts/                (legado, mantido como alternativa)
    └── stories/
```

| Arquivo/Pasta | Função | Por quê existe |
|---|---|---|
| `docker-compose.yml` | Sobe o sandbox .NET isolado | Build/test nunca rodam no seu host — nem risco de "funciona na minha máquina", nem risco do agente estragar seu ambiente local |
| `mcp-server/` | Servidor MCP com as tools (`dotnet_build`, `dotnet_test`, `dotnet_format_check`, `state_read`, `state_write`, `scope_check`) | Protocolo **aberto** — qualquer cliente MCP usa, não é exclusivo de nenhum produto |
| `.mcp.json` | Aponta o cliente (Claude Code etc.) para o servidor MCP acima | Formato padrão MCP, na raiz porque é onde os clientes procuram |
| `AGENT.md` | Regras que o agente lê: stack, convenções, Definition of Done, o que pode/não pode alterar | Texto puro — funciona com qualquer agente capaz de ler arquivo, não é um "hook" proprietário |
| `ONBOARDING.md` | Checklist rodado **uma vez** por repositório, pro agente mapear convenções reais antes de tocar em código | Essencial em brownfield — evita o agente assumir padrões de projeto novo num código legado |
| `agents.config.json` | Mapeia `--agent claude/copilot/devin` para o comando real de cada CLI (flags, registro de MCP) | Único ponto de manutenção quando a sintaxe de um CLI muda — dev nunca precisa editar isso no dia a dia |
| `harness.js` | Orquestrador do loop, em Node.js puro (sem `sh`/`bash`) | Elimina de vez problemas de `PATH`/shell quebrado; funciona igual em Linux/macOS/Windows |
| `scripts/*.sh` | Versão em bash do mesmo orquestrador | Mantida como alternativa/comparação, não é mais o caminho recomendado |
| `skills/*.md` | Convenções reutilizáveis do time (padrão de endpoint, padrão de teste, etc.) | Evita repetir a mesma convenção em toda história; `AGENT.md` instrui o agente a consultar a skill relevante antes de começar |
| `stories/*.md` | Histórias de usuário, formato Given/When/Then + seção de Escopo | Formato testável, com "Escopo" explícito pra impedir o agente de alterar arquivos fora do combinado |
| `stories/.state-<slug>.md` | Memória externa por história (gerado automaticamente) | Sobrevive a `/compact`, reinício de sessão, ou troca de agente no meio da história |
| `stories/.baseline.md` | Snapshot de quantos testes já passavam **antes** da história (brownfield) | Define "regressão" de forma objetiva — história não pode reduzir esse número |

---

## Como instalar num repositório real

1. Copie a pasta `.harness/` inteira para a raiz do seu repositório.
2. Copie `.mcp.json` também para a raiz (fora de `.harness/`).
3. Em `.harness/docker-compose.yml`, ajuste:
   - `image: mcr.microsoft.com/dotnet/sdk:X.0` → a versão real do seu .NET
   - `working_dir: /workspace/<caminho-até-a-.sln>` → onde sua solution realmente está
4. Em `.harness/AGENT.md`, ajuste a seção "Stack" para refletir seu projeto de
   verdade (framework de teste, ORM, versão do C#, etc.)
5. Instale as dependências do servidor MCP:
   ```
   cd .harness/mcp-server && npm install
   ```
6. Suba o sandbox:
   ```
   cd .harness
   node harness.js setup
   ```
7. **Se o repositório já existe (brownfield):** peça ao agente para seguir
   `ONBOARDING.md` uma vez, depois rode `node harness.js baseline`.

---

## Como usar no dia a dia

```bash
cd .harness

# Criar uma história nova
node harness.js new-story minha-feature

# Ver quais agentes estão configurados e instalados
node harness.js agents

# Rodar — cada dev só troca o --agent (e opcionalmente --model)
node harness.js run stories/minha-feature.md --agent claude
node harness.js run stories/minha-feature.md --agent claude --model opus
node harness.js run stories/minha-feature.md --agent copilot --model claude-sonnet-4.5
node harness.js run stories/minha-feature.md --agent devin --model sonnet

# Testar de graça, 100% local, com Ollama (via Aider)
node harness.js run stories/minha-feature.md --max-iter 30 -- \
  aider --model ollama/qwen2.5-coder:14b --yes-always --no-auto-commits ../src
```

O que acontece a cada iteração do loop, sempre:
1. O agente escolhido recebe o prompt (ler `AGENT.md` + a história + retomar estado)
2. Ele edita código/testes
3. `harness.js` roda `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`
   **dentro do container**, de forma independente do que o agente afirmou
4. Se tudo passar (e, em brownfield, se não houver regressão vs. `baseline.md`),
   encerra. Senão, repete até `--max-iter`.

---

## Decisões importantes que valem lembrar

- **Escopo explícito em cada história** ("pode alterar" / "não pode alterar") existe
  pra impedir o agente de "aproveitar" pra refatorar código fora do pedido — isso é
  a causa nº 1 de PR gigante e impossível de revisar em projetos brownfield.
- **Suposições ambíguas viram texto explícito na história**, não decisão silenciosa
  do agente (ex: "busca por nome é exata ou parcial?") — evita retrabalho quando a
  suposição errada só aparece na revisão.
- **`--` no `harness.js` permite comando customizado**, ignorando `agents.config.json`
  por completo — é o modo avançado/escape hatch, útil pra ferramentas fora da lista
  (Aider+Ollama, ou qualquer CLI novo) sem precisar editar nada.

## O que ficou fora, por decisão consciente (por ora)

- **Pipeline multi-agente** (planejador → executor → testador → reporter →
  coordenador) — desenhamos a viabilidade, mas decidimos não construir agora porque
  o custo/latência só compensa em histórias complexas o suficiente pra um único
  agente "se convencer" de que terminou sem cobrir os casos de borda.

## Limitações honestas

- Modelos locais (Ollama) são bem mais fracos que os CLIs pagos em loops de TDD
  longos — útil pra validar a mecânica do harness, não pra produção real.
- A sintaxe exata de `copilot` e `devin` CLI muda com frequência (produtos recentes,
  evoluindo rápido) — o `_nota` em cada entrada de `agents.config.json` já aponta
  onde checar se algo parar de funcionar.
- O loop valida build/test/format, mas não substitui revisão humana antes do merge —
  o harness para no branch local, nunca dá push sozinho.
