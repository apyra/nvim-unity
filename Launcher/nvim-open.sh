#!/bin/bash

FILE="$1"
LINE="${2:-1}"
SERVER="${3:-http://localhost:42069}"
CONFIG_FILE="$(dirname "$0")/config.json"
ROOT="$4"

# Muda para a raiz do projeto (importante pro Neovim abrir com o contexto certo)
cd "$ROOT" || exit

# Detecta sistema operacional (Linux ou OSX)
OS="$(uname)"
[[ "$OS" == "Darwin" ]] && OS="OSX"

# Lê terminal do config.json (requer jq instalado)
TERMINAL=$(jq -r ".terminals.\"$OS\"" "$CONFIG_FILE")

if [ -z "$TERMINAL" ]; then
  echo "[nvim-open] Terminal não encontrado para $OS"
  exit 1
fi

# Verifica se o servidor está rodando
if curl --silent --fail --max-time 1 "$SERVER/status" > /dev/null; then
  echo "[nvim-open] Enviando para $SERVER/open"
  echo "$FILE:$LINE" | curl -s -X POST "$SERVER/open" -H "Content-Type: text/plain" --data-binary @-
else
  echo "[nvim-open] Servidor não encontrado, abrindo diretamente com Neovim..."
  "$TERMINAL" -e nvim "$FILE" +$LINE
fi

