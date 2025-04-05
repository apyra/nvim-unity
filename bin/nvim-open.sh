#!/bin/bash

FILE=$1

# Check if nvim server is running
if nvr --serverlist | grep -q "nvim"; then
  nvr --remote-tab "$FILE"
else
  nvim --listen /tmp/nvim "$FILE"
fi

