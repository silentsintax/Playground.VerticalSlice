# Harness portável — desenvolvimento de histórias .NET (sem vendor lock-in)

Requisitos: Docker Desktop rodando, Node.js 18+, e algum agente CLI de sua escolha
(Claude Code, aider, ou outro compatível com MCP ou com prompt não-interativo).

## Onde isso fica no seu repositório

Recomendado: uma pasta oculta `.harness/` na raiz do seu repositório, ao lado do
código real. Exemplo, se seu código está em `src/`:

```
meu_repo_git/
├── .git/
├── .mcp.json              <- fica na raiz (Claude Code procura aqui)
├── src/
│   └── MeuProjeto.sln
└── .harness/               <- todo o conteúdo deste pacote entra aqui
    ├── AGENT.md
    ├── ONBOARDING.md
    ├── docker-compose.yml
    ├── mcp-server/
    ├── scripts/
    └── stories/
```

Os arquivos já vêm pré-configurados para esse layout:
- `docker-compose.yml` monta `..` (a raiz do repo, um nível acima de `.harness/`)
  como `/workspace`, e define `working_dir: /workspace/src` — ajuste `src` se sua
  `.sln` estiver em outro caminho.
- `.mcp.json` (que deve ficar na RAIZ do repo, fora de `.harness/`) já aponta para
  `mcp-server/index.js` com `cwd: ".harness"`.

Se preferir outro layout (harness na raiz sem pasta oculta, ou em repositório
separado), basta ajustar esses dois arquivos — a lógica não muda.

Rode os comandos dos scripts sempre a partir de dentro de `.harness/`:
```
cd .harness
./scripts/setup.sh
```

## Setup

1. Confirme que seu código já está no lugar de sempre (`src/`, ou onde estiver) —
   nada precisa ser copiado ou movido.
2. Instale as dependências do servidor MCP:
   ```
   cd .harness/mcp-server && npm install && cd ..
   ```
3. Suba o sandbox:
   ```
   ./scripts/setup.sh
   ```

## Uso com um agente que suporta MCP nativamente (ex: Claude Code)

Aponte o agente para o `.mcp.json` deste diretório — ele vai enxergar as tools
`dotnet_build`, `dotnet_test`, `dotnet_format_check`, `state_read`, `state_write` e
`scope_check` automaticamente. Peça para o agente ler `AGENT.md` e desenvolver a
história em `stories/<slug>.md` seguindo o processo descrito lá.

Isso funciona com qualquer cliente compatível com MCP — não é exclusivo de nenhum
produto. A troca de agente, se um dia fizer sentido, não exige reescrever nenhuma
lógica: as tools continuam as mesmas.

## Uso com um agente que só aceita CLI não-interativo (ex: sem suporte a MCP)

Use o orquestrador, que roda o loop e valida o Definition of Done de forma
independente do agente (direto no container, via `docker exec`):

```
AGENT_CMD="claude -p" ./scripts/orchestrator.sh stories/minha-historia.md
```

Troque `AGENT_CMD` pelo comando do seu agente de preferência. O script:
1. Monta um prompt apontando pro `AGENT.md` e pra história
2. Chama o agente
3. Verifica de forma independente se build + test + format estão OK dentro do container
4. Repete até passar ou até `MAX_ITER` (padrão 15) esgotar

## Uso sem scripts shell (recomendado se seu terminal tiver problemas de PATH/bash)

Este harness também tem uma versão da orquestração em Node.js puro
(`harness.js`), que substitui todos os scripts `.sh`. Nenhum comando passa por
`sh`/`bash`/`cmd` — cada programa (`docker`, `dotnet`, o agente) é chamado
diretamente, então problemas de `PATH` quebrado ou shell mal configurado deixam
de ser um fator.

```bash
# Setup inicial (equivalente a scripts/setup.sh)
node harness.js setup

# Captura baseline (equivalente a scripts/baseline.sh)
node harness.js baseline

# Cria nova história (equivalente a scripts/new-story.sh)
node harness.js new-story minha-feature

# Roda o loop (equivalente a scripts/orchestrator.sh)
# Tudo depois de "--" é o comando do agente, já em argumentos separados —
# não há parsing de string, então não existe problema de aspas/escape.
node harness.js run stories/minha-feature.md --max-iter 30 -- \
  aider --model ollama/qwen2.5-coder:14b --yes-always --no-auto-commits ../src
```

Com Claude Code (ou outro que aceite prompt não-interativo via `-p`):
```bash
node harness.js run stories/minha-feature.md -- claude -p
```

Os scripts em `scripts/*.sh` continuam no pacote como alternativa equivalente,
mas `harness.js` é a opção mais portátil — funciona igual em Linux, macOS e
Windows (sem precisar de WSL ou Git Bash), já que só depende do Node.js, que
você já precisa ter instalado para o `mcp-server`.



Para testar o harness inteiro offline, usando um modelo local via Ollama em vez de
Claude Code ou qualquer API paga:

1. Instale o [Ollama](https://ollama.com/download) e o [Aider](https://aider.chat)
   (Aider é o "agente" que sabe editar arquivos e conversar com Ollama — o Ollama
   sozinho só gera texto, não edita código).
2. Rode o setup:
   ```
   ./scripts/setup-ollama.sh qwen2.5-coder:14b
   ```
   Troque o modelo pelo que sua máquina aguenta. `qwen2.5-coder` e
   `deepseek-coder-v2` são as famílias mais confiáveis para tarefas agenticas de
   código hoje; modelos genéricos (llama base, mistral base) tendem a se perder no
   loop de TDD.
3. Rode o orquestrador normalmente, apontando pro wrapper:
   ```
   AGENT_CMD="./scripts/ollama-agent.sh qwen2.5-coder:14b" MAX_ITER=30 \
     ./scripts/orchestrator.sh stories/sua-historia.md
   ```
   `MAX_ITER` mais alto que o padrão (15) porque modelos locais tipicamente precisam
   de mais tentativas para chegar a uma solução correta.

### O que muda em relação a usar Claude Code
- O Aider edita os arquivos direto no host (não via MCP) — funciona porque o volume
  do Docker já é compartilhado, então build/test dentro do container enxergam as
  mudanças normalmente.
- As tools de `state_read`/`state_write` do servidor MCP não são usadas nesse modo;
  o `AGENT.md` funciona só como texto de prompt para o Aider, sem integração de
  memória estruturada.
- Espere qualidade bem inferior a Claude em tarefas de TDD multi-passo — é útil para
  validar a mecânica do harness (loop, validação independente, baseline) sem custo,
  mas não é comparável em capacidade de resolver histórias complexas sozinho.



```
./scripts/new-story.sh nome-da-historia
```

## Por que isso não tem vendor lock-in

- **MCP** é um protocolo aberto — o servidor em `mcp-server/` funciona com qualquer
  cliente que o implemente.
- **Docker Compose** é padrão de mercado, roda em qualquer máquina com Docker Desktop.
- **AGENT.md** e os arquivos de estado são markdown puro, lidos por qualquer agente
  capaz de ler arquivo.
- O **orquestrador** é bash puro — troca de agente é só trocar a variável `AGENT_CMD`.

Trocar de "Claude Code" para outro agente não exige reescrever hooks, commands ou
subagentes — só reconfigurar qual comando é chamado.
