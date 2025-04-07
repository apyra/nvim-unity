#!/bin/bash

FILE="$1"
LINE="$2"
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIG_FILE="$DIR/config.json"
OS="$(uname -s)"
TERMINAL="x-terminal-emulator"

# Detect platform
case "$OS" in
    Darwin)   PLATFORM="mac" ;;
    Linux)    PLATFORM="linux" ;;
    *)        PLATFORM="unknown" ;;
esac

# Read config
if [ -f "$CONFIG_FILE" ]; then
    TERMINAL_FROM_CONFIG=$(jq -r ".terminal.$PLATFORM // empty" "$CONFIG_FILE")
    if [ -n "$TERMINAL_FROM_CONFIG" ]; then
        TERMINAL="$TERMINAL_FROM_CONFIG"
    fi
fi

# Open
"$TERMINAL" -e nvim "$FILE" +$LINE

