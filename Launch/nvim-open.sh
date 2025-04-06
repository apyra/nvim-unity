#!/bin/sh
FILE="$1"
LINE="$2"

if command -v nvr >/dev/null 2>&1; then
    nvr --remote-tab "$FILE" "$LINE"
else
    nvim "$FILE" "$LINE"
fi

