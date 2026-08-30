#!/usr/bin/env node
/**
 * CLI do harness — substitui os scripts .sh por Node.js puro.
 * Nenhum comando aqui passa por um shell (sh/bash/cmd) — cada programa
 * (docker, dotnet, o agente) é chamado diretamente via spawn com array de
 * argumentos. Isso elimina de vez problemas de PATH/shell quebrado.
 *
 * Uso:
 *   node harness.js setup
 *   node harness.js baseline
 *   node harness.js new-story <slug>
 *   node harness.js run <arquivo-da-historia.md> [--max-iter N] -- <comando-do-agente...>
 *
 * Exemplo de run:
 *   node harness.js run stories/feature-get-by-name.md --max-iter 30 -- \
 *     aider --model ollama/qwen2.5-coder:14b --yes-always --no-auto-commits
 *
 * Tudo depois de "--" é o comando do agente, já separado em argumentos —
 * não há parsing de string de shell, então aspas/escapes não são um problema.
 */

import { spawn } from "node:child_process";
import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const HARNESS_DIR = path.dirname(fileURLToPath(import.meta.url));
const CONTAINER = "dotnet-story-sandbox";

// ---------- Helpers de execução (sem shell) ----------

function run(cmd, args, opts = {}) {
  return new Promise((resolve) => {
    const child = spawn(cmd, args, { stdio: "inherit", cwd: HARNESS_DIR, ...opts });
    child.on("error", (err) => {
      console.error(`Falha ao executar '${cmd}': ${err.message}`);
      resolve(false);
    });
    child.on("close", (code) => resolve(code === 0));
  });
}

function runCapture(cmd, args, opts = {}) {
  return new Promise((resolve) => {
    let output = "";
    const child = spawn(cmd, args, { cwd: HARNESS_DIR, ...opts });
    child.stdout?.on("data", (d) => (output += d.toString()));
    child.stderr?.on("data", (d) => (output += d.toString()));
    child.on("error", (err) => resolve({ ok: false, output: err.message }));
    child.on("close", (code) => resolve({ ok: code === 0, output }));
  });
}

function dockerExec(args) {
  // Chama o binário direto DENTRO do container, sem "bash -lc" — não
  // precisa de shell nem no host nem no container para isso funcionar.
  return runCapture("docker", ["exec", CONTAINER, ...args]);
}

// ---------- Comandos ----------

async function cmdSetup() {
  console.log("Subindo container sandbox...");
  const up = await run("docker", ["compose", "up", "-d"]);
  if (!up) {
    console.error("Falha ao subir o container. Docker Desktop está rodando?");
    process.exit(1);
  }
  console.log("Restaurando pacotes NuGet dentro do container...");
  const restore = await dockerExec(["dotnet", "restore"]);
  console.log(restore.output);
  console.log(restore.ok ? "Setup concluído." : "Restore falhou — verifique o output acima.");
}

async function cmdBaseline() {
  console.log("Rodando suíte de testes para capturar baseline...");
  const result = await dockerExec(["dotnet", "test", "--nologo"]);
  const total = (result.output.match(/Total:\s*(\d+)/) || [])[1] ?? "?";
  const passed = (result.output.match(/Passed:\s*(\d+)/) || [])[1] ?? "?";
  const failed = (result.output.match(/Failed:\s*(\d+)/) || [])[1] ?? "0";
  const skipped = (result.output.match(/Skipped:\s*(\d+)/) || [])[1] ?? "0";

  const content = `# Baseline de testes (capturado antes de qualquer história nova)

Data: ${new Date().toISOString()}

- Total: ${total}
- Passando: ${passed}
- Falhando: ${failed}
- Ignorados/Skip: ${skipped}

## Regra de uso
Nenhuma história pode reduzir o número de "Passando" abaixo deste valor.
`;

  await fs.mkdir(path.join(HARNESS_DIR, "stories"), { recursive: true });
  await fs.writeFile(path.join(HARNESS_DIR, "stories", ".baseline.md"), content, "utf-8");
  console.log(content);
}

async function cmdNewStory(slug) {
  if (!slug) {
    console.error("Uso: node harness.js new-story <slug>");
    process.exit(1);
  }
  const dest = path.join(HARNESS_DIR, "stories", `${slug}.md`);
  try {
    await fs.access(dest);
    console.error(`Já existe: ${dest}`);
    process.exit(1);
  } catch {
    // não existe, pode criar
  }
  const template = await fs.readFile(path.join(HARNESS_DIR, "stories", "_template.md"), "utf-8");
  await fs.writeFile(dest, template, "utf-8");
  console.log(`Criado: ${dest}`);
}

async function loadAgentsConfig() {
  const raw = await fs.readFile(path.join(HARNESS_DIR, "agents.config.json"), "utf-8");
  const config = JSON.parse(raw);
  delete config._comentario;
  return config;
}

async function agentIsInstalled(agentConfig) {
  const res = await runCapture(agentConfig.command, agentConfig.checkArgs || ["--version"]);
  return res.ok || res.output.length > 0; // alguns CLIs retornam código != 0 em --version
}

async function ensureMcpRegistered(agentName, agentConfig) {
  if (!agentConfig.mcpSetup) return; // agente descobre MCP sozinho (ex: Claude Code)

  const { checkArgs, checkContains, addArgs } = agentConfig.mcpSetup;
  const check = await runCapture(agentConfig.command, checkArgs);
  if (check.output.includes(checkContains)) {
    return; // já registrado, não faz nada
  }

  console.log(`Registrando MCP server para '${agentName}' (primeira vez apenas)...`);
  const add = await runCapture(agentConfig.command, addArgs);
  if (!add.ok) {
    console.warn(
      `Aviso: não foi possível registrar o MCP server automaticamente para '${agentName}'.\n` +
        `Saída: ${add.output.slice(-500)}\n` +
        `Verifique a sintaxe atual com '${agentConfig.command} mcp --help' e ajuste ` +
        `'mcpSetup.addArgs' em agents.config.json se necessário. Prosseguindo mesmo assim.`
    );
  }
}


  const build = await dockerExec(["dotnet", "build", "-warnaserror", "--nologo"]);
  if (!build.ok) return { ok: false, stage: "build", output: build.output };

  const test = await dockerExec(["dotnet", "test", "--nologo"]);
  if (!test.ok) return { ok: false, stage: "test", output: test.output };

  const format = await dockerExec(["dotnet", "format", "--verify-no-changes"]);
  if (!format.ok) return { ok: false, stage: "format", output: format.output };

  return { ok: true };
}

async function cmdRun(storyFile, maxIter, agentName, modelName, rawAgentParts) {
  if (!storyFile) {
    console.error(
      "Uso: node harness.js run <historia.md> --agent <nome> [--max-iter N]\n" +
        "  ou: node harness.js run <historia.md> [--max-iter N] -- <comando-do-agente...>\n" +
        "Agentes configurados: node harness.js agents"
    );
    process.exit(1);
  }

  let buildArgs; // função (prompt) => [comando, ...args]

  if (rawAgentParts.length > 0) {
    // Modo avançado: comando explícito após "--", ignora agents.config.json
    const [cmd, ...args] = rawAgentParts;
    buildArgs = (prompt) => [cmd, [...args, prompt]];
  } else {
    if (!agentName) {
      console.error("Especifique --agent <nome> ou use '-- <comando>'. Veja: node harness.js agents");
      process.exit(1);
    }
    const agents = await loadAgentsConfig();
    const agentConfig = agents[agentName];
    if (!agentConfig) {
      console.error(
        `Agente '${agentName}' não está em agents.config.json. Disponíveis: ${Object.keys(agents).join(", ")}`
      );
      process.exit(1);
    }
    if (!(await agentIsInstalled(agentConfig))) {
      console.error(
        `Comando '${agentConfig.command}' não encontrado no PATH. Instale/autentique o ` +
          `CLI de '${agentName}' antes de continuar.`
      );
      process.exit(1);
    }
    await ensureMcpRegistered(agentName, agentConfig);

    const modelArgs =
      modelName && agentConfig.modelFlag ? [agentConfig.modelFlag, modelName] : [];
    if (modelName && !agentConfig.modelFlag) {
      console.warn(
        `Aviso: '${agentName}' não tem 'modelFlag' definido em agents.config.json — ` +
          `o --model informado será ignorado.`
      );
    }

    buildArgs = (prompt) => [
      agentConfig.command,
      [...modelArgs, ...agentConfig.args.map((a) => a.replace("{prompt}", prompt))],
    ];
  }

  console.log(
    `== Desenvolvendo história: ${storyFile} (agente: ${agentName || "customizado"}` +
      (modelName ? `, modelo: ${modelName}` : "") +
      ") =="
  );

  for (let i = 1; i <= maxIter; i++) {
    console.log(`\n--- Iteração ${i}/${maxIter} ---`);

    const prompt =
      `Leia AGENT.md e siga o processo descrito lá para a história em ${storyFile}. ` +
      `Iteração ${i}/${maxIter}. Se existir estado em stories/.state-*, retome de lá. ` +
      `Ao final, atualize o estado com o progresso feito.`;

    const [cmd, args] = buildArgs(prompt);
    await run(cmd, args);

    const dod = await checkDefinitionOfDone();
    if (dod.ok) {
      console.log(`\nDefinition of Done atingido na iteração ${i}.`);
      return;
    }
    console.log(`Falhou em '${dod.stage}':\n${dod.output.slice(-1500)}`);
  }

  console.log(`\nLimite de ${maxIter} iterações atingido sem fechar o Definition of Done.`);
  process.exit(1);
}

async function cmdAgents() {
  const agents = await loadAgentsConfig();
  console.log("Agentes configurados em agents.config.json:\n");
  for (const [name, cfg] of Object.entries(agents)) {
    const installed = (await agentIsInstalled(cfg)) ? "instalado" : "NÃO encontrado no PATH";
    console.log(`  ${name.padEnd(10)} comando: ${cfg.command.padEnd(10)} [${installed}]`);
  }
}

// ---------- Parsing de argumentos (sem depender de libs externas) ----------

async function main() {
  const [, , command, ...rest] = process.argv;

  if (command === "setup") {
    await cmdSetup();
  } else if (command === "baseline") {
    await cmdBaseline();
  } else if (command === "new-story") {
    await cmdNewStory(rest[0]);
  } else if (command === "agents") {
    await cmdAgents();
  } else if (command === "run") {
    const dashIndex = rest.indexOf("--");
    const beforeDash = dashIndex === -1 ? rest : rest.slice(0, dashIndex);
    const agentParts = dashIndex === -1 ? [] : rest.slice(dashIndex + 1);

    const maxIterFlagIndex = beforeDash.indexOf("--max-iter");
    const maxIter = maxIterFlagIndex !== -1 ? parseInt(beforeDash[maxIterFlagIndex + 1], 10) : 15;

    const agentFlagIndex = beforeDash.indexOf("--agent");
    const agentName = agentFlagIndex !== -1 ? beforeDash[agentFlagIndex + 1] : undefined;

    const modelFlagIndex = beforeDash.indexOf("--model");
    const modelName = modelFlagIndex !== -1 ? beforeDash[modelFlagIndex + 1] : undefined;

    // Primeiro argumento posicional (não é uma flag "--x" nem o valor logo após uma) é o arquivo da história
    const flagValuePositions = new Set(
      [maxIterFlagIndex, agentFlagIndex, modelFlagIndex]
        .filter((i) => i !== -1)
        .map((i) => i + 1)
    );
    const positional = beforeDash.find(
      (a, idx) => !a.startsWith("--") && !flagValuePositions.has(idx)
    );

    await cmdRun(positional, maxIter, agentName, modelName, agentParts);
  } else {
    console.log(`Comandos disponíveis:
  node harness.js setup
  node harness.js baseline
  node harness.js new-story <slug>
  node harness.js agents
  node harness.js run <historia.md> --agent <nome> [--model <nome-do-modelo>] [--max-iter N]
  node harness.js run <historia.md> [--max-iter N] -- <comando-do-agente...>  (modo avançado)`);
    process.exit(1);
  }
}

main();
