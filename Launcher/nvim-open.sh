#!/bin/bash

FILE="$1"
LINE="${2:-1}"
SERVER="$3"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
CONFIG_FILE="$SCRIPT_DIR/config.json"

# Detecta o sistema operacional
OS_TYPE="$(uname)"
if [[ "$OS_TYPE" == "Darwin" ]]; then
  OS_KEY="OSX"
else
  OS_KEY="Linux"
fi

# Extrai terminal do config.json
TERMINAL=$(grep "\"$OS_KEY\"" "$CONFIG_FILE" | cut -d':' -f2 | tr -d ' ",')
if [ -z "$TERMINAL" ]; then
  echo "[nvim-open] Terminal não encontrado no config.json para $OS_KEY"
  exit 1
fi

# Verifica se o servidor está rodando
if curl --silent --fail --max-time 1 "$SERVER/status" > /dev/null; then
  echo "[nvim-open] Enviando para o servidor $SERVER/open"
  echo "$FILE:$LINE" | curl -s -X POST "$SERVER/open" -H "Content-Type: text/plain" --data-binary @-
else
  echo "[nvim-open] Servidor não encontrado, abrindo diretamente com $TERMINAL..."
  "$TERMINAL" nvim "$FILE" +"$LINE"
fi


