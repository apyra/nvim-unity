# Apyra.nvim-unity

## Overview
[NvimUnity]("https://github.com/apyra/nvim-unity.git") is a Unity integration designed to make Neovim the default external script editor for Unity. It allows you to open `.cs` files in Neovim directly from the Unity Editor and regenerate project files, just like popular editors like VSCode.

## Features
- Open Unity C# files in Neovim directly from the Unity Editor
- Preserves line number and project context (working directory)
- Regenerate `.csproj` files from Neovim
- Multiplatform support: Windows, Linux, macOS

## Optional Configuration
For the command :Uopen (Open Unity from Neovim)
```lua
require("unity.plugin").setup({
  unity_path = "/path/to/Unity/Editor/Unity.exe" -- Optional override
})
```
### You can change your terminal in `config.json` 
```json
{
  "terminals": {
    "Windows": "wt",
    "Linux": "gnome-terminal",
    "OSX": "iTerm"
  }
}
```

MIT


