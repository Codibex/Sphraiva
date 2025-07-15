# Sphraiva Architecture Overview

Sphraiva is a modular C#/.NET application designed for local operation, implementing the Model Context Protocol (MCP) and Retrieval-Augmented Generation (RAG). The architecture consists of several main components:

- **MCP.WebApp**: The Blazor-based web front-end for user interaction.
- **MCP.Host**: Provides API endpoints and manages communication between agents and the server.
- **MCP.Server**: Handles core backend logic, agent orchestration, and workflow management.
- **LLM (Ollama Container)**: Large Language Model running in a dedicated Docker container for inference tasks.
- **MCP.BackgroundWorker.FileSystem**: Responsible for file system ingestion and embedding generation. It operates independently and does not interact directly with the MCP.Server component.
- **Qdrant**: Vector database for storing and searching embeddings.
- **Agent Containers**: Run in Docker containers and execute tasks as directed by the server.

The following diagram illustrates the high-level architecture and interactions between these components:

```mermaid
flowchart TB
    subgraph Web
        MCPWebApp[MCP.WebApp<br>Blazor UI]
    end
    subgraph Backend
        MCPHost[MCP.Host<br>API Endpoints]
        MCPServer[MCP.Server<br>Core Logic & Orchestration]
        OllamaLLM[LLM<br>Ollama Container]
        BackgroundWorker[MCP.BackgroundWorker.FileSystem<br>File Ingestion & Embedding]
        Qdrant[Qdrant<br>Vector DB]
    end
    subgraph AgentContainers
        AgentContainer1[Agent Container 1<br>Docker]
        AgentContainer2[Agent Container 2<br>Docker]
    end

    MCPWebApp --> MCPHost
    MCPHost --> MCPServer
    MCPHost --> OllamaLLM
    MCPHost --> Qdrant
    MCPServer --> AgentContainer1
    MCPServer --> AgentContainer2
    BackgroundWorker --> Qdrant
```
