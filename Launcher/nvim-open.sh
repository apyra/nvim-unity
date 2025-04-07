#!/bin/bash

FILE="$1"
LINE="${2:-1}"
SERVER="${3:-http://localhost:5005}"

# Verifica se o servidor está rodando
if curl --silent --fail --max-time 1 "${SERVER}status" > /dev/null; then
  echo "[nvim-open] Enviando para o servidor: ${SERVER}open"
  printf "%s:%s" "$FILE" "$LINE" | curl -s -X POST "${SERVER}open" -H "Content-Type: text/plain" --data-binary @-
else
  echo "[nvim-open] Servidor não encontrado, abrindo diretamente com nvim..."
  nvim "$FILE" +$LINE
fi

