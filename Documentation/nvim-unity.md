# Apyra.nvim-unity

## Overview
[NvimUnity]("https://github.com/apyra/nvim-unity.git") is a Unity integration designed to make Neovim the default external script editor for Unity. It allows you to open `.cs` files in Neovim directly from the Unity Editor and regenerate project files, just like popular editors like VSCode.

---

## Features
- Open Unity C# files in Neovim directly from the Unity Editor
- Preserves line number and project context (working directory)
- Regenerate `.csproj` files from Neovim
- Multiplatform support: Windows, Linux, macOS

---

### Regenerate Project Files
- From Unity: `Preferences > External Tools > Regenerate Project Files`
- From Assets Menu: `Tools > Neovim Code Editor > Regenerate Project Files`

### Custom Terminal

*If you want to run nvim from a custom terminal you can set it here. In windows for example if you run nvim directly, it may not showup very well because the current user will not be bound to it.

![Custom Terminal Preferences](ExternalTools.png)

---

MIT


