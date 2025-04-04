# Nvim Unity Editor

This Unity package integrates Neovim as an external script editor and provides a Regenerate Project Files button inside the Unity Editor.

## 📦 Features

- Open `.cs` files directly in Neovim from Unity.
- Adds a menu item to regenerate `.csproj` files like in VSCode.
- Installable via Unity Package Manager.

## 📂 Installation

### Option 1: Local

1. Download this repository as a `.zip`.
2. Extract into `Packages/nvim-unity-editor` inside your Unity project.

### Option 2: Git (recommended)

```json
https://github.com/apyra/nvim-unity-editor.git
```

### Set Neovim as External Editor

1. Go to `Edit > Preferences > External Tools`
2. Set External Script Editor to: `Packages/nvim-unity-editor/bin/nvim-open.bat`

## 🧠 Regenerate Project Files

Go to: `Assets > NvimUnity > Regenerate Project Files`

This triggers recompilation and can help restore `.csproj` files.

---

MIT License.