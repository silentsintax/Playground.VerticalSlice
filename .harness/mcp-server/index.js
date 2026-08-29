#!/usr/bin/env node
/**
 * Servidor MCP local para desenvolvimento de histórias .NET.
 * Não depende de nenhum vendor de agente — qualquer cliente MCP pode
 * conectar nele (stdio) e usar as ferramentas abaixo.
 *
 * Todas as execuções de dotnet rodam DENTRO do container Docker
 * (dotnet-story-sandbox), nunca no host.
 */

import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
} from "@modelcontextprotocol/sdk/types.js";
import { execFile } from "node:child_process";
import { promisify } from "node:util";
import fs from "node:fs/promises";
import path from "node:path";

const execFileAsync = promisify(execFile);
const CONTAINER = "dotnet-story-sandbox";
const PROJECT_ROOT = path.resolve(process.cwd(), "project");
const STORIES_DIR = path.resolve(process.cwd(), "stories");

// ---------- Helpers ----------

async function dockerExec(cmd) {
  try {
    const { stdout, stderr } = await execFileAsync(
      "docker",
      ["exec", CONTAINER, "bash", "-lc", cmd],
      { maxBuffer: 1024 * 1024 * 20 }
    );
    return { ok: true, output: (stdout + stderr).trim() };
  } catch (err) {
    // err.stdout/err.stderr contêm o output mesmo em falha
    const raw = `${err.stdout || ""}\n${err.stderr || ""}`.trim();
    return { ok: false, output: raw || err.message };
  }
}

// Reduz o log bruto do dotnet a só as linhas relevantes de erro,
// para economizar tokens no agente que consome esta tool.
function extractBuildErrors(output) {
  const lines = output.split("\n").filter((l) => /error CS|error MSB/.test(l));
  return lines.length ? lines.join("\n") : output.split("\n").slice(-40).join("\n");
}

function extractTestFailures(output) {
  const lines = output.split("\n");
  const idx = lines.findIndex((l) => /Failed!|FAILED/.test(l));
  if (idx === -1) return lines.slice(-40).join("\n");
  return lines.slice(Math.max(0, idx - 5), idx + 30).join("\n");
}

async function stateFilePath(slug) {
  return path.join(STORIES_DIR, `.state-${slug}.md`);
}

// ---------- Definição das tools MCP ----------

const TOOLS = [
  {
    name: "dotnet_build",
    description:
      "Roda 'dotnet build -warnaserror' dentro do container sandbox. Retorna apenas as linhas de erro relevantes (CSxxxx/MSBxxxx), não o log inteiro.",
    inputSchema: { type: "object", properties: {}, required: [] },
  },
  {
    name: "dotnet_test",
    description:
      "Roda 'dotnet test' dentro do container sandbox. Retorna apenas o trecho relevante de falhas, não o log inteiro.",
    inputSchema: { type: "object", properties: {}, required: [] },
  },
  {
    name: "dotnet_format_check",
    description:
      "Roda 'dotnet format --verify-no-changes'. Retorna se há divergências de formatação pendentes.",
    inputSchema: { type: "object", properties: {}, required: [] },
  },
  {
    name: "dotnet_format_apply",
    description: "Aplica 'dotnet format' para corrigir formatação automaticamente.",
    inputSchema: { type: "object", properties: {}, required: [] },
  },
  {
    name: "state_read",
    description:
      "Lê o arquivo de estado (memória externa) de uma história, em stories/.state-<slug>.md. Use para retomar progresso sem depender do histórico da conversa.",
    inputSchema: {
      type: "object",
      properties: { slug: { type: "string", description: "slug da história" } },
      required: ["slug"],
    },
  },
  {
    name: "state_write",
    description:
      "Grava/atualiza o arquivo de estado de uma história. Deve ser chamado após cada critério de aceite concluído.",
    inputSchema: {
      type: "object",
      properties: {
        slug: { type: "string" },
        content: { type: "string", description: "Conteúdo markdown completo do estado" },
      },
      required: ["slug", "content"],
    },
  },
  {
    name: "scope_check",
    description:
      "Verifica sobreposição de escopo entre múltiplas histórias (seção '## Escopo' de cada arquivo .md em stories/). Use antes de desenvolver histórias em paralelo.",
    inputSchema: {
      type: "object",
      properties: {
        storyFiles: {
          type: "array",
          items: { type: "string" },
          description: "Caminhos relativos dos arquivos de história a comparar",
        },
      },
      required: ["storyFiles"],
    },
  },
];

// ---------- Implementação das tools ----------

async function handleTool(name, args) {
  switch (name) {
    case "dotnet_build": {
      const res = await dockerExec("dotnet build -warnaserror --nologo");
      return res.ok
        ? { ok: true, message: "Build OK." }
        : { ok: false, message: extractBuildErrors(res.output) };
    }

    case "dotnet_test": {
      const res = await dockerExec(
        'dotnet test --nologo --logger "console;verbosity=normal"'
      );
      return res.ok
        ? { ok: true, message: "Todos os testes passaram." }
        : { ok: false, message: extractTestFailures(res.output) };
    }

    case "dotnet_format_check": {
      const res = await dockerExec("dotnet format --verify-no-changes");
      return res.ok
        ? { ok: true, message: "Formatação OK, nada pendente." }
        : { ok: false, message: "Divergências de formatação encontradas:\n" + res.output.slice(-2000) };
    }

    case "dotnet_format_apply": {
      const res = await dockerExec("dotnet format");
      return { ok: res.ok, message: res.ok ? "Formatação aplicada." : res.output };
    }

    case "state_read": {
      const file = await stateFilePath(args.slug);
      try {
        const content = await fs.readFile(file, "utf-8");
        return { ok: true, message: content };
      } catch {
        return { ok: false, message: `Nenhum estado encontrado para '${args.slug}'. É uma história nova.` };
      }
    }

    case "state_write": {
      const file = await stateFilePath(args.slug);
      await fs.mkdir(path.dirname(file), { recursive: true });
      await fs.writeFile(file, args.content, "utf-8");
      return { ok: true, message: `Estado salvo em ${file}` };
    }

    case "scope_check": {
      const scopes = {};
      for (const f of args.storyFiles) {
        const full = path.resolve(process.cwd(), f);
        const content = await fs.readFile(full, "utf-8");
        const match = content.match(/## Escopo([\s\S]*?)(\n## |$)/);
        const paths = match
          ? [...match[1].matchAll(/`([^`]+)`/g)].map((m) => m[1])
          : [];
        scopes[f] = paths;
      }
      const conflicts = [];
      const files = Object.keys(scopes);
      for (let i = 0; i < files.length; i++) {
        for (let j = i + 1; j < files.length; j++) {
          for (const p1 of scopes[files[i]]) {
            for (const p2 of scopes[files[j]]) {
              if (p1.startsWith(p2) || p2.startsWith(p1)) {
                conflicts.push(`${files[i]} <-> ${files[j]} (caminho: ${p1} / ${p2})`);
              }
            }
          }
        }
      }
      return conflicts.length
        ? { ok: false, message: "Conflitos de escopo:\n" + conflicts.join("\n") }
        : { ok: true, message: "Nenhuma sobreposição — seguro paralelizar." };
    }

    default:
      throw new Error(`Tool desconhecida: ${name}`);
  }
}

// ---------- Bootstrap do servidor MCP ----------

const server = new Server(
  { name: "dotnet-story-harness", version: "1.0.0" },
  { capabilities: { tools: {} } }
);

server.setRequestHandler(ListToolsRequestSchema, async () => ({ tools: TOOLS }));

server.setRequestHandler(CallToolRequestSchema, async (request) => {
  const { name, arguments: args } = request.params;
  const result = await handleTool(name, args ?? {});
  return {
    content: [{ type: "text", text: result.message }],
    isError: !result.ok,
  };
});

const transport = new StdioServerTransport();
await server.connect(transport);
