# Sphraiva Modernization Plan
*Last Updated: February 1, 2026*

---

## Table of Contents

- [Part I: Introduction](#part-i-introduction)
  - [1. Overview](#1-overview)
  - [2. Terminology Glossary](#2-terminology-glossary)
- [Part II: Technology Landscape](#part-ii-technology-landscape)
  - [3. Industry Standards & Protocols](#3-industry-standards--protocols)
  - [4. Agent Definition Approaches](#4-agent-definition-approaches)
  - [5. Configuration & Prompt Management](#5-configuration--prompt-management)
  - [6. Capabilities & Integrations](#6-capabilities--integrations)
  - [7. .NET Technology Stack](#7-net-technology-stack)
- [Part III: Implementation Plan](#part-iii-implementation-plan)
  - [8. Architecture & Project Structure](#8-architecture--project-structure)
  - [9. Implementation Phases](#9-implementation-phases)
  - [10. Success Criteria & Risks](#10-success-criteria--risks)
- [Appendices](#appendices)

---

# Part I: Introduction

## 1. Overview

### 1.1 Document Purpose

This modernization plan outlines the strategic upgrade of Sphraiva's agentic development environment to align with February 2026 industry standards.

### 1.2 Target Audience

**Primary**: .NET developers building agentic applications using:
- Microsoft Semantic Kernel (recommended for local LLM support)
- Microsoft Agent Framework (for Azure-first deployments)
- Multi-agent orchestration systems

**Scope**: Agent application development (using frameworks to build agents)

### 1.3 Related Documentation

| Topic | Resource |
|-------|----------|
| Agent Skills Standard | [agentskills.io](https://agentskills.io/) |
| Project Documentation for Agents | [agents.md](https://agents.md/) |
| AutoGen/LangGraph Frameworks | Framework documentation |

---

## 2. Terminology Glossary

> **Important**: Clear terminology prevents confusion across frameworks.

| Term | Definition | Context |
|------|------------|---------|
| **Function** | Callable unit with parameters (JSON Schema) | OpenAI API, Semantic Kernel |
| **Tool** | Broader capability (includes functions, search, APIs) | MCP, Agent frameworks |
| **Plugin** | Collection of related functions | Semantic Kernel |
| **Agent Skill** | Portable capability package (SKILL.md) | agentskills.io standard |
| **Agent Config** | Runtime settings (prompts, model, tools) | YAML/JSON specs |
| **AGENTS.md** | Project documentation FOR agents | Repository root |

---

# Part II: Technology Landscape

## 3. Industry Standards & Protocols

### 3.1 Model Context Protocol (MCP)

MCP is the **2026 standard for tool integration** - it defines how agents connect to external capabilities.

**Key Points:**
- **Purpose**: Server/tool registration (NOT agent configuration)
- **Format**: JSON-based configuration (`mcp.json`, `claude_desktop_config.json`)
- **Transport**: HTTP/SSE or stdio with JSON-RPC messages
- **Adoption**: Microsoft, Anthropic, Databricks

**Configuration Example:**
```json
{
  "mcpServers": {
    "filesystem": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-filesystem"]
    }
  }
}
```

### 3.2 Agent-to-Agent Protocol (A2A)

- Enables inter-agent communication and delegation
- Used by CrewAI, Microsoft frameworks
- `AgentCard` JSON format for agent discovery

### 3.3 Standard Interfaces

**JSON Schema** is the universal standard for function definitions across all major frameworks.

---

## 4. Agent Definition Approaches

### 4.1 The 2026 Paradigm: Layered Configuration

> **Clarification**: The trend is toward **layered configuration** - code-first for agent logic, external files for orchestration and environment-specific settings.

**Configuration Layer Model:**

| Layer | Format | Purpose | Best Practice |
|-------|--------|---------|---------------|
| **Agent Logic** | Code (C#/Python) | Core behavior, functions | Code-first, type-safe |
| **Instructions** | Markdown/YAML | System prompts, guidelines | External files, version-controlled |
| **Orchestration** | YAML | Team/workflow definitions | Declarative specs |
| **Runtime** | JSON/appsettings | Environment config | Externalized, per-environment |

### 4.2 Framework Comparison

| Framework | Primary Approach | Config Files | Status |
|-----------|------------------|--------------|--------|
| **Semantic Kernel** | Code-first + YAML orchestration | YAML (experimental) | Core: Stable, Orchestration: Experimental |
| **Microsoft Agent Framework** | Code-first + declarative | YAML agent specs | Production-ready (Azure-focused) |
| **CrewAI** | YAML + Code hybrid | agents.yaml, tasks.yaml | Mature |
| **LangGraph** | Pure code orchestration | None | Mature |
| **AutoGen** | Hybrid (Pydantic models) | Python configs | Mature |

### 4.3 Best Practices Summary

**✅ Recommended:**

1. **Code-First for Agent Logic**
   - Define agents in code using framework SDKs
   - Type-safe function definitions with `[KernelFunction]` attributes

2. **External Files for Configuration**
   - System prompts and instructions in Markdown files
   - YAML for team/workflow orchestration
   - JSON for runtime settings (appsettings.json)

3. **Hybrid Pattern** (Recommended for Sphraiva):
   ```
   src/
     agents/
       FileSystemAgent.cs     # Code: agent logic
   config/
     agents/
       filesystem-agent.yaml  # YAML: orchestration
     prompts/
       filesystem/
         instructions.md      # Markdown: system prompt
   ```

**❌ Avoid:**
- Standalone config files without framework integration
- Hardcoded prompts in source code
- Over-engineered configuration without clear purpose

### 4.4 Standard File Organization

```
project/
├── config/
│   ├── agents/              # Agent orchestration specs
│   ├── prompts/             # System prompts & templates
│   └── models/              # Model configurations
├── src/
│   ├── Agents/              # Agent implementations
│   ├── Tools/               # Function/tool definitions
│   └── Workflows/           # Orchestration logic
└── mcp.json                 # MCP server config
```

---

## 5. Configuration & Prompt Management

### 5.1 Agent Configuration Files

> **Recommendation**: External configuration files are a **best practice** for production agents.

**Benefits:**
- Version control & change tracking
- Separation of concerns (no recompilation needed)
- Team collaboration (non-developers can edit prompts)
- Environment-specific overrides
- Audit compliance

**Standard YAML Format:**
```yaml
name: FileSystemAgent
description: Manages file operations
template_format: semantic-kernel
instructions: |
  You are a file system assistant.
  ## Capabilities
  - Read and write files
  - List directories
  ## Guidelines
  - Validate all paths
  - Confirm destructive operations
model:
  id: gpt-4o-mini
  temperature: 0.3
tools:
  - FileSystemPlugin.ReadFile
  - FileSystemPlugin.WriteFile
```

**When to Use External Files vs. Inline:**

| Scenario | External Files | Inline |
|----------|----------------|--------|
| Production agents | ✅ | |
| Team collaboration | ✅ | |
| Compliance required | ✅ | |
| Quick prototyping | | ✅ |
| Educational samples | | ✅ |

### 5.2 Prompt Templates

Prompt files remain essential with standardized formats and sophisticated management.

**YAML Prompt Schema:**
```yaml
name: QueryDocuments
description: RAG query template
template: |
  Use the following context to answer the question.
  
  Context: {{context}}
  Question: {{query}}
template_format: handlebars
input_variables:
  - name: context
    is_required: true
  - name: query
    is_required: true
execution_settings:
  default:
    model_id: gpt-4o-mini
    temperature: 0.5
```

**Template Engines Supported:**
- Handlebars (`{{variable}}`)
- Liquid
- Jinja (Azure AI Foundry)
- Semantic-kernel native

### 5.3 Version Control & Lifecycle

**Recommended Workflow:**
1. **Author** → Write YAML with Markdown formatting
2. **Version** → Commit to Git with semantic versioning
3. **Review** → Peer review process
4. **Test** → Evaluate against test datasets
5. **Deploy** → CI/CD to App Configuration
6. **Monitor** → Track effectiveness with telemetry
7. **Iterate** → Version based on monitoring

**Tools:**
- Git-based versioning (standard)
- MLflow Prompt Registry (advanced tracking)
- Azure AI Foundry Gallery (templates)

---

## 6. Capabilities & Integrations

### 6.1 Tools & Plugins (Framework-Specific)

**Semantic Kernel Structure:**
```
Kernel
└─ Plugins (collections)
    └─ Functions (KernelFunction)
        ├─ Native functions (C# code)
        ├─ Prompt functions (templates)
        └─ OpenAPI functions (imported)
```

**MCP Server Structure:**
```
MCP Server
├─ Tools (executable functions)
├─ Resources (data sources)
└─ Prompts (templates)
```

**Registration (Semantic Kernel):**
```csharp
// From type
kernel.Plugins.AddFromType<FileSystemPlugin>();

// From OpenAPI
await kernel.ImportPluginFromOpenApiAsync("api", spec);
```

### 6.2 Agent Skills (agentskills.io)

> **Distinct from framework tools**: Agent Skills are portable capability packages.

**Key Characteristics:**
- Format: SKILL.md (YAML frontmatter + Markdown)
- Portability: Works across 15+ agent products
- Purpose: Domain expertise, not programmatic actions

**SKILL.md Example:**
```yaml
---
name: code-review
description: "Code review with best practices"
license: Apache-2.0
---

# Code Review Skill

## When to Use
Use when reviewing pull requests.

## Process
1. Check code style
2. Verify test coverage
3. Review security
```

**Supported Products:** Claude Code, GitHub, VS Code, Spring AI, Databricks, and more.

### 6.3 AGENTS.md (Project Documentation)

> **Purpose**: Documentation FOR coding agents (like README for AI)

**What to Include:**
```markdown
# AGENTS.md

## Setup
- Install: `dotnet restore`
- Build: `dotnet build`
- Test: `dotnet test`

## Architecture
- MCP.Host = API gateway
- MCP.WebApp = Blazor frontend

## Code Style
- Nullable reference types
- Microsoft naming conventions
```

**Supported By:** GitHub Copilot, Claude Code, Cursor, Devin, Windsurf, and more.

### 6.4 Capability Composition

**Use All Three Together:**

| Layer | Technology | Purpose |
|-------|------------|---------|
| Knowledge | Agent Skills | Domain expertise, workflows |
| Orchestration | Semantic Kernel | Agent coordination |
| Execution | SK Plugins | Programmatic actions |
| Integration | MCP Tools | External services |

**Best Practices:**
- Limit tools per agent (≤20)
- Use MCP for shared capabilities
- Package plugins as NuGet packages
- Document all tools

---

## 7. .NET Technology Stack

### 7.1 Runtime Selection

| Version | Status | Support End | Recommendation |
|---------|--------|-------------|----------------|
| **.NET 10** | Latest Stable (Nov 2025) | Nov 2028 (LTS) | ✅ **Recommended** |
| .NET 9 | Previous | May 2026 | Migrate to .NET 10 |
| .NET 11 | Preview | — | Not for production |

> **Action**: Upgrade Sphraiva to .NET 10 for LTS support.

### 7.2 Framework Selection

| Framework | Best For | LLM Support | Status |
|-----------|----------|-------------|--------|
| **Semantic Kernel** | Local LLM projects | Ollama, vLLM, llama.cpp, OpenAI, Azure | ✅ Core: Stable, Orchestration: Experimental |
| Agent Framework | Azure-first projects | Azure OpenAI only | Production-ready |

**For Sphraiva**: Use **Semantic Kernel** (local LLM support via Ollama, vLLM, or llama.cpp).

### 7.3 Alternative LLM Backends (vLLM, llama.cpp)

In addition to Ollama, **vLLM** or **llama.cpp** can be used as local LLM backends. Both provide OpenAI-compatible APIs, enabling integration with Semantic Kernel via the OpenAI connector.

#### vLLM

**Advantages:**
- Very high performance through PagedAttention
- Continuous batching for multiple concurrent requests
- GPU-optimized (CUDA, ROCm)
- Supports many models (Llama, Mistral, Qwen, etc.)

**Docker Setup:**
```dockerfile
FROM vllm/vllm-openai:latest

ENV MODEL_NAME=mistralai/Ministral-3B-Instruct
ENV VLLM_API_PORT=8000

ENTRYPOINT ["python", "-m", "vllm.entrypoints.openai.api_server", \
            "--model", "${MODEL_NAME}", \
            "--port", "${VLLM_API_PORT}", \
            "--host", "0.0.0.0"]
```

**docker-compose.yml:**
```yaml
sphraiva-vllm:
  image: vllm/vllm-openai:latest
  container_name: sphraiva-vllm
  ports:
    - "8000:8000"
  environment:
    - HUGGING_FACE_HUB_TOKEN=${HF_TOKEN}
  volumes:
    - vllm-models:/root/.cache/huggingface
  command: >
    --model mistralai/Ministral-3B-Instruct
    --port 8000
    --host 0.0.0.0
  deploy:
    resources:
      reservations:
        devices:
          - driver: nvidia
            count: 1
            capabilities: [gpu]
```

**Semantic Kernel Integration:**
```csharp
// vLLM provides OpenAI-compatible API
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(
        modelId: "mistralai/Ministral-3B-Instruct",
        apiKey: "not-needed",  // Local server
        endpoint: new Uri("http://localhost:8000/v1"))
    .Build();
```

#### llama.cpp (llama-server)

**Advantages:**
- Very lightweight, CPU-optimized
- GGUF format for quantized models
- Low memory footprint
- Simple installation

**Docker Setup:**
```dockerfile
FROM ghcr.io/ggerganov/llama.cpp:server

ENV MODEL_PATH=/models/model.gguf
ENV LLAMA_API_PORT=8080

ENTRYPOINT ["llama-server", \
            "--model", "${MODEL_PATH}", \
            "--port", "${LLAMA_API_PORT}", \
            "--host", "0.0.0.0", \
            "-c", "8192"]  # Context length
```

**docker-compose.yml:**
```yaml
sphraiva-llamacpp:
  image: ghcr.io/ggerganov/llama.cpp:server
  container_name: sphraiva-llamacpp
  ports:
    - "8080:8080"
  volumes:
    - ./models:/models:ro
  command: >
    --model /models/ministral-3b-instruct-q4_k_m.gguf
    --port 8080
    --host 0.0.0.0
    --ctx-size 8192
    --n-gpu-layers 99
```

**Semantic Kernel Integration:**
```csharp
// llama.cpp server provides OpenAI-compatible API under /v1
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(
        modelId: "ministral-3b",  // Arbitrary name
        apiKey: "not-needed",
        endpoint: new Uri("http://localhost:8080/v1"))
    .Build();
```

#### Backend Comparison

| Feature | Ollama | vLLM | llama.cpp |
|---------|--------|------|-----------|
| **Primary Use Case** | Simplicity | Performance | Lightweight |
| **API Format** | Ollama + OpenAI-compatible | OpenAI-compatible | OpenAI-compatible |
| **Model Format** | GGUF, Safetensors | Safetensors, AWQ, GPTQ | GGUF |
| **GPU Support** | Good | Excellent | Good |
| **CPU Support** | Good | Limited | Excellent |
| **Multi-Request** | Good | Excellent (Batching) | Good |
| **Memory Efficiency** | Good | Very Good | Excellent |
| **Setup Complexity** | Very Easy | Medium | Easy |
| **SK Connector** | Dedicated | OpenAI Connector | OpenAI Connector |

#### Recommended Configuration

**For Maximum Performance (GPU):**
```yaml
# appsettings.json
{
  "LLM": {
    "Provider": "vLLM",
    "Endpoint": "http://sphraiva-vllm:8000/v1",
    "ModelId": "mistralai/Ministral-3B-Instruct",
    "ApiKey": ""
  }
}
```

**For Lower Resource Usage (CPU/Mixed):**
```yaml
# appsettings.json
{
  "LLM": {
    "Provider": "LlamaCpp",
    "Endpoint": "http://sphraiva-llamacpp:8080/v1",
    "ModelId": "ministral-3b",
    "ApiKey": ""
  }
}
```

**Generic Service Registration:**
```csharp
public static class LLMServiceExtensions
{
    public static IKernelBuilder AddLLMChatCompletion(
        this IKernelBuilder builder,
        IConfiguration config)
    {
        var provider = config["LLM:Provider"];
        var endpoint = config["LLM:Endpoint"];
        var modelId = config["LLM:ModelId"];
        var apiKey = config["LLM:ApiKey"] ?? string.Empty;

        return provider switch
        {
            "Ollama" => builder.AddOllamaChatCompletion(modelId, new Uri(endpoint)),
            "vLLM" or "LlamaCpp" => builder.AddOpenAIChatCompletion(
                modelId, apiKey, endpoint: new Uri(endpoint)),
            "AzureOpenAI" => builder.AddAzureOpenAIChatCompletion(
                modelId, endpoint, new DefaultAzureCredential()),
            _ => throw new ArgumentException($"Unknown LLM provider: {provider}")
        };
    }
}

// Usage
var kernel = Kernel.CreateBuilder()
    .AddLLMChatCompletion(configuration)
    .Build();
```

### 7.4 Core Libraries

```xml
<!-- Essential packages -->
<PackageReference Include="Microsoft.SemanticKernel" />
<PackageReference Include="Microsoft.SemanticKernel.Connectors.Ollama" />
<PackageReference Include="Microsoft.SemanticKernel.Connectors.OpenAI" /> <!-- For vLLM/llama.cpp -->
<PackageReference Include="Microsoft.Extensions.AI.Abstractions" />

<!-- Optional: Azure integration -->
<PackageReference Include="Azure.AI.OpenAI" />
<PackageReference Include="Azure.Identity" />
```

### 7.5 Patterns & Architecture

**Dependency Injection:**
```csharp
builder.Services.AddKernel();
builder.Services.AddSingleton<IFileSystemAgent, FileSystemAgent>();
```

**Builder Pattern:**
```csharp
var kernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("llama3.2", new Uri("http://localhost:11434"))
    .Build();
kernel.Plugins.AddFromType<FileSystemPlugin>();
```

**Async/Await (Essential):**
```csharp
// Single invocation
var response = await kernel.InvokeAsync("plugin", "function", args);

// Streaming
await foreach (var chunk in kernel.InvokeStreamingAsync(...))
{
    Console.Write(chunk);
}
```

### 7.6 Azure Integration

| Service | Purpose |
|---------|---------|
| Azure OpenAI | Cloud LLM provider |
| Azure AI Foundry | Persistent agents, managed services |
| Azure App Configuration | Centralized config |
| Azure Key Vault | Secrets management |
| Azure Monitor | Observability |

**Authentication:**
```csharp
var credential = new DefaultAzureCredential();
// Works with Managed Identity, Azure CLI, etc.
```

### 7.7 Deployment Options

| Platform | Use Case |
|----------|----------|
| Azure Container Apps | Microservices (recommended) |
| Azure App Service | Web applications |
| Azure Functions | Serverless, durable agents |
| Azure Kubernetes (AKS) | Large-scale orchestration |
| Self-hosted | On-premises, multi-cloud |

**Docker Base Image:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
```

---

# Part III: Implementation Plan

## 8. Architecture & Project Structure

### 8.1 Recommended Directory Layout

```
src/
├── Agents/
│   ├── Core/
│   │   ├── BaseAgent.cs
│   │   └── AgentFactory.cs
│   └── Specialized/
│       ├── FileSystemAgent/
│       │   ├── agent.yaml
│       │   ├── instructions.md
│       │   └── FileSystemAgent.cs
│       └── RAGAgent/
├── Tools/
│   ├── Core/
│   │   └── ToolBase.cs
│   ├── FileSystem/
│   │   ├── FileSystemPlugin.cs
│   │   └── tool-schema.json
│   └── VectorSearch/
├── MCPServers/
│   └── FileSystemMCP/
├── Workflows/
│   ├── Pipelines/
│   └── Orchestrations/
└── Shared/
    ├── Models/
    └── Extensions/
config/
├── agents/
│   └── filesystem-agent.yaml
├── prompts/
│   └── rag/
│       └── query-documents.yaml
└── mcp-servers/
    └── mcp-config.json
```

### 8.2 Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Plugins/Functions | PascalCase, Verb-Noun | `ReadFile`, `SendEmail` |
| Files | `{Name}Plugin.cs`, `{Name}Agent.yaml` | `FileSystemPlugin.cs` |
| MCP Servers | kebab-case | `github-mcp-server` |

---

## 9. Implementation Phases

### Phase 1: Foundation (Weeks 1-2)

**Goal**: Upgrade infrastructure and restructure project.

| Task | Details |
|------|---------|
| Upgrade to .NET 10 | Update all `.csproj` files |
| Add Semantic Kernel packages | Core + Ollama connector |
| Update Docker images | `mcr.microsoft.com/dotnet/aspnet:10.0` |
| Create new directory structure | As defined in 8.1 |

### Phase 2: Agent Modernization (Weeks 3-4)

**Goal**: Implement modern agent patterns.

| Task | Details |
|------|---------|
| Configure Ollama connector | Local LLM support |
| Create agent YAML definitions | External instruction files |
| Implement plugin functions | `[KernelFunction]` attributes |
| Add agent factory | Load agents from YAML |

**Example - Agent Definition:**
```yaml
# config/agents/filesystem-agent.yaml
name: FileSystemAgent
description: Manages file operations
instructions: |
  You are a file system assistant.
  - Validate all paths
  - Confirm destructive operations
model:
  id: llama3.2
  temperature: 0.3
tools:
  - FileSystemPlugin.ReadFile
  - FileSystemPlugin.WriteFile
```

### Phase 3: Tool Development (Weeks 5-6)

**Goal**: Modernize plugins and implement MCP.

| Task | Details |
|------|---------|
| Convert Plugins to Tools | Add `[Description]` attributes |
| Create JSON schemas | OpenAPI format |
| Implement MCP servers | For shared tools |
| Document all tools | README per tool |

**Example - Tool Implementation:**
```csharp
public class FileSystemPlugin
{
    [KernelFunction]
    [Description("Reads file contents")]
    public async Task<string> ReadFile(
        [Description("Path to file")] string path)
    {
        return await File.ReadAllTextAsync(path);
    }
}
```

### Phase 4: Prompt Management (Week 7)

**Goal**: Implement versioned prompt templates.

| Task | Details |
|------|---------|
| Create prompt YAML files | Organized by domain |
| Implement template loader | Support Handlebars |
| Add prompt versioning | Git-based with tags |
| Create prompt tests | Evaluation framework |

### Phase 5: Multi-Agent Orchestration (Weeks 8-9)

**Goal**: Implement orchestration patterns.

| Task | Details |
|------|---------|
| Sequential pipelines | Agent-to-agent handoffs |
| Concurrent execution | Fan-out/fan-in |
| Group chat | Collaborative agents |
| Durable agents | Azure Functions (optional) |

### Phase 6: Deployment (Week 10)

**Goal**: Production-ready deployment.

| Task | Details |
|------|---------|
| Environment-based config | appsettings.{Environment}.json |
| OpenTelemetry | Observability |
| Update Docker configs | Health checks, graceful shutdown |
| CI/CD pipelines | Automated testing/deployment |

### Phase 7: Testing & Documentation (Week 11)

**Goal**: Comprehensive testing and docs.

| Task | Details |
|------|---------|
| Unit tests | >80% coverage |
| Integration tests | End-to-end workflows |
| Update README | Modern architecture |
| Create AGENTS.md | For coding agents |
| API documentation | Swagger/OpenAPI |

### Phase 8: Security & Compliance (Week 12)

**Goal**: Enterprise-grade security.

| Task | Details |
|------|---------|
| Managed Identity | Azure authentication |
| Content safety | Azure Content Safety |
| RBAC | Role-based access |
| Audit logging | All agent actions |
| Security documentation | Compliance requirements |

---

## 10. Success Criteria & Risks

### 10.1 Success Metrics

| Category | Metric | Target |
|----------|--------|--------|
| **Technical** | Agents migrated | 100% |
| | Tool schemas defined | 100% |
| | Test coverage | >80% |
| **Performance** | Response time (p95) | <2s |
| | Tool success rate | >95% |
| | Uptime | >99.9% |
| **Quality** | Security vulnerabilities | 0 critical |
| | Documentation | >90% complete |

### 10.2 Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Breaking changes during migration | Feature flags, parallel run |
| SK API changes (experimental) | Pin package versions, monitor releases |
| Performance degradation | Benchmark before/after |
| Deployment complexity | Staged rollout |

### 10.3 Resource Requirements

**Team**: 1-2 Senior .NET developers, 1 DevOps engineer, 1 QA engineer

**Timeline**: 12 weeks (+ 2 weeks buffer)

---

# Appendices

## A. References

| Resource | URL |
|----------|-----|
| Semantic Kernel | https://learn.microsoft.com/en-us/semantic-kernel/ |
| SK Ollama Connector | https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/ollama |
| SK OpenAI Connector | https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/ |
| vLLM Documentation | https://docs.vllm.ai/ |
| vLLM OpenAI Server | https://docs.vllm.ai/en/latest/serving/openai_compatible_server.html |
| llama.cpp | https://github.com/ggerganov/llama.cpp |
| llama.cpp Server | https://github.com/ggerganov/llama.cpp/blob/master/examples/server/README.md |
| Model Context Protocol | https://modelcontextprotocol.io/ |
| Azure AI Foundry | https://learn.microsoft.com/en-us/azure/ai-foundry/ |
| .NET AI Development | https://learn.microsoft.com/en-us/dotnet/ai/ |
| Agent Skills | https://agentskills.io/ |
| AGENTS.md Spec | https://agents.md/ |

## B. Sphraiva-Specific Actions

- [ ] Create `AGENTS.md` in repository root
- [ ] Create `skills/` directory for Agent Skills
- [ ] Upgrade all projects to .NET 10
- [ ] Implement Semantic Kernel with configurable LLM backend (Ollama/vLLM/llama.cpp)
- [ ] Evaluate vLLM vs llama.cpp for performance requirements
- [ ] Create agent YAML definitions
- [ ] Implement MCP servers for shared tools
- [ ] Add OpenTelemetry observability

---

*Document Version: 2.0*  
*Last Updated: February 1, 2026*  
*Status: Restructured - Resolved contradictions, unified terminology*
