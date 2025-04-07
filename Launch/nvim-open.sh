#!/bin/bash

FILE="$1"
LINE="$2"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIG_FILE="$SCRIPT_DIR/config.json"
TERMINAL="gnome-terminal"

if [ -f "$CONFIG_FILE" ]; then
  OS="$(uname)"
  case "$OS" in
    Darwin) OS_KEY="OSX" ;;
    Linux) OS_KEY="Linux" ;;
    *) OS_KEY="Unknown" ;;
  esac

  TERMINAL=$(jq -r ".terminals[\"$OS_KEY\"]" "$CONFIG_FILE" 2>/dev/null)
  if [ "$TERMINAL" == "null" ] || [ -z "$TERMINAL" ]; then
    TERMINAL="gnome-terminal"
  fi
fi

# Executa no terminal
$TERMINAL -- bash -c "nvim --listen nvim-unity \"$FILE\" +$LINE"




# #!/bin/bash
#
# FILE="$1"
# LINE="$2"
# DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# CONFIG_FILE="$DIR/config.json"
# OS="$(uname -s)"
# TERMINAL="x-terminal-emulator"
#
# # Detect platform
# case "$OS" in
#     Darwin)   PLATFORM="mac" ;;
#     Linux)    PLATFORM="linux" ;;
#     *)        PLATFORM="unknown" ;;
# esac
#
# # Read config
# if [ -f "$CONFIG_FILE" ]; then
#     TERMINAL_FROM_CONFIG=$(jq -r ".terminal.$PLATFORM // empty" "$CONFIG_FILE")
#     if [ -n "$TERMINAL_FROM_CONFIG" ]; then
#         TERMINAL="$TERMINAL_FROM_CONFIG"
#     fi
# fi
#
# # Open
# "$TERMINAL" -e nvim "$FILE" +$LINE

