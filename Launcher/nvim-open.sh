#!/bin/bash

TERMINAL="$1"
shift

ARGS="$@"

if [ "$TERMINAL" = "iterm" ]; then
    osascript <<EOF
tell application "iTerm"
    create window with default profile
    tell current session of current window
        write text "nvim $ARGS"
    end tell
end tell
EOF

elif [ "$TERMINAL" = "wt" ]; then
    $TERMINAL cmd /k "nvim $ARGS"

else
    $TERMINAL -e "nvim $ARGS"
fi

