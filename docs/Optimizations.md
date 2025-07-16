# Optimizations & Recommendations

## Semantic Kernel

### 1. Kernel Initialization and Dependency Injection (DI) ✅

- **Problem:** You use `Kernel.CreateBuilder()` and build the kernel directly before adding it to DI. Semantic Kernel recommends registering services (embedding generators, chat clients, etc.) via DI and building the kernel in the DI context so all dependencies are resolved correctly.
- **Recommendation:** Use the new extensions like `services.AddSemanticKernel(...)` (if available) and build the kernel only after all services are registered. This improves testability and flexibility.

### 2. Synchronous Waiting on Async

- **Problem:** `RegisterMcp(kernel).GetAwaiter().GetResult();` blocks synchronously on an async call. This can cause deadlocks, especially in ASP.NET environments.
- **Recommendation:** Register MCP tools asynchronously in Startup, e.g., via a HostedService or by making kernel initialization fully async.

### 3. Service Lifetime

- **Problem:** You register the kernel as a singleton. This is correct, but check if all dependent services (e.g., HttpClient) are also registered correctly as singleton or scoped to avoid resource leaks.

### 4. VectorStore and TextSearch

- **Problem:** Using `AddKeyedTransient<VectorStoreTextSearch<TextParagraph>>` is correct, but ensure all mappers and search providers are injected via DI instead of direct instantiation.
- **Recommendation:** Register mappers as services and inject them to improve testability and interchangeability.

### 5. Configuration

- **Problem:** You use `configuration["OLLAMA_SERVER"]!` etc. without fallback or validation.
- **Recommendation:** Use the Options pattern (`IOptions<T>`) for configuration and validate settings at startup.

---

## MCP (Model Context Protocol)

### 1. Tool and Resource Registration

- **Problem:** MCP tools are registered at startup, but there is no error handling if the connection fails.
- **Recommendation:** Add logging and retry mechanisms if the MCP server is unreachable.

### 2. SseClientTransport

- **Problem:** The endpoint URL is hardcoded.
- **Recommendation:** Get the URL from configuration and validate it.

---

## Semantic Kernel Documentation: Deviations

- **Synchronous Waiting on Async:** The docs recommend not using `.GetAwaiter().GetResult()` or `.Result` on tasks in Startup.
- **DI Integration:** The docs show how to register kernel and plugins via DI and extensions instead of manual construction.
- **Error Handling:** The docs recommend catching and logging errors when loading plugins/tools.

---

## Concrete Improvement Suggestions

1. **Async Initialization for MCP Tools**
   - Use a HostedService to register MCP tools asynchronously after startup.
2. **Mapper and Provider via DI**
   - Register `TextParagraphTextSearchStringMapper` and `TextParagraphTextSearchResultMapper` as services.
3. **Configuration via Options**
   - Create an `OllamaSettings` class and register it with `services.Configure<OllamaSettings>(...)`.
4. **Error Handling and Logging**
   - Add logging for all critical initialization steps.
