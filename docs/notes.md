# Notes

Embeddings: Parameter:

## LLM Data

| LLM                                 | Context | Embedding Dimension | Ollama Link | Tools Support |
|-------------------------------------|---------|---------------------|             |               |
| devstral                            | 128k    | 5120                |             |               |
| interstellarninja/llama3.1-8b-tools | 8k      | 4096?               |             |               |
| gpt-oss:20b                         | 128k    | Not supported       |             |               |
| qwen3:14b                           | 128k    | 5120?               |             |               |
| granite3.3                          | 128k    | ?                   | [granite3.3](https://ollama.com/library/granite3.3) |               |
| deepseek-r1:8b                      | 128k    | ?                   | [deepseek-r1](https://ollama.com/library/deepseek-r1) | no |

## Docker

### Docker image updates

The following steps update all images of the docker compose services

1. Stop and remove containers

   ```powershell
   docker compose down
   ```

2. docker compose pull

   ```powershell
   docker compose pull
   ```

3. Start containers with new images (optional)

   ```powershell
   docker compose up -d
   ```
