#!/bin/bash

FILE="$1"
LINE="${2:-1}"

# Verifica se o servidor está rodando
if curl --silent --fail --max-time 1 http://localhost:5005/status > /dev/null; then
  # Envia requisição para o servidor
  echo "$FILE:$LINE" | curl -X POST http://localhost:5005/open -H "Content-Type: text/plain" --data-binary @-
else
  echo "[nvim-open] Servidor não encontrado, abrindo diretamente com nvim..."
  nvim "$FILE" +$LINE
fi

