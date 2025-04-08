#!/bin/bash

FILE="$1"
LINE="${2:-1}"
SERVER="${3:-http://localhost:42069}"
CONFIG_FILE="$(dirname "$0")/config.json"
ROOT="$4"

cd "$ROOT" || exit

OS="$(uname)"
[[ "$OS" == "Darwin" ]] && OS="OSX"

TERMINAL=$(jq -r ".terminals.\"$OS\"" "$CONFIG_FILE")

if [ -z "$TERMINAL" ]; then
  echo "[nvim-open] Terminal não encontrado para $OS"
  exit 1
fi

if curl --silent --fail --max-time 1 "$SERVER/status" > /dev/null; then
  echo "[nvim-open] Enviando para $SERVER/open"
  echo "$FILE:$LINE" | curl -s -X POST "$SERVER/open" -H "Content-Type: text/plain" --data-binary @-
else
  echo "[nvim-open] Servidor não encontrado, abrindo diretamente com Neovim..."
  "$TERMINAL" -e nvim -c "let g:unity_server = '$SERVER'" "$FILE" +$LINE
fi

