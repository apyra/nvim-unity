# Apyra.nvim-unity

## Overview
[NvimUnity]("https://github.com/apyra/nvim-unity.git") is a Unity integration designed to make Neovim the default external script editor for Unity. It allows you to open `.cs` files in Neovim directly from the Unity Editor and regenerate project files, just like popular editors like VSCode.

## Features
- Open Unity C# files in Neovim directly from the Unity Editor
- Preserves line number and project context (working directory)
- Regenerate `.csproj` files from Neovim
- Multiplatform support: Windows, Linux, macOS

## Installation

### Unity
1. Install the Unity package from Git:

```json
https://github.com/apyra/nvim-unity.git
```
### Neovim Plugin
Install [nvim-unity-handle]("https://github.com/apyra/nvim-unity-handle.git") via your preferred plugin manager:
```lua
{
  "apyra/nvim-unity-handle",
  config = function()
    require("unity.plugin").setup()
  end,
}
```

## Optional Configuration
For the command :Uopen (Open Unity from Neovim)
```lua
require("unity.plugin").setup({
  unity_path = "/path/to/Unity/Editor/Unity.exe" -- Optional override
})
```
Set a global variable if you want to override the default Unity server address:
```lua
vim.g.unity_server = "http://localhost:42069"
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

## Unity Side
- `NvimUnityServer` will start automatically when Unity opens.
- HTTP endpoints:
  - `POST /open` — opens a file in Neovim
  - `POST /regenerate` — regenerates project files
  - `GET /status` — check if the server is alive

## License
MIT


