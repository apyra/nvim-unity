#!/bin/bash

FILE="$1"
LINE="$2"
SOCKET="$3"
ROOT="$4"
ISPROJECTOPEN="$5"

# Garante que tenha uma linha
if [ -z "$LINE" ]; then
  LINE=1
fi

# Pega o terminal a partir do config.json
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIG_FILE="$SCRIPT_DIR/config.json"
TERMINAL=$(jq -r '.terminals.Linux // .terminals.OSX' "$CONFIG_FILE")

# Escape para <CR>
ESC=$'\x1b'
GOTO_LINE="${LINE}${ESC}"

if [ "$ISPROJECTOPEN" = "true" ]; then
  "$TERMINAL" -e bash -c \
    "nvim --server \"$SOCKET\" --remote \"$FILE\" && nvim --server \"$SOCKET\" --remote-send \"$GOTO_LINE\""
else
  "$TERMINAL" -e bash -c \
    "cd \"$ROOT\" && nvim --listen \"$SOCKET\" \"$FILE\" +$LINE"
fi

