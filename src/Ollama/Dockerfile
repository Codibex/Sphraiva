FROM ollama/ollama:latest

ENV OLLAMA_MODEL=${OLLAMA_MODEL}

# Modell beim Start laden
ENTRYPOINT ["sh", "-c", "ollama serve & sleep 5 && ollama pull $OLLAMA_MODEL && wait"]