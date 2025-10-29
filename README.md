![Nvim Unity Banner](Documentation/Banner.png)

# NvimUnity

<p align="center">
  <img alt="Neovim Plugin" src="https://img.shields.io/badge/Neovim-Plugin-57A143?logo=neovim&logoColor=white">
  <a href="https://github.com/rockerBOO/awesome-neovim">
    <img alt="Awesome" src="https://awesome.re/badge.svg">
  </a>
  <img alt="License: MIT" src="https://img.shields.io/badge/License-MIT-yellow.svg">
</p>

This Unity package integrates Neovim as an external script editor and provides a **Regenerate Project Files** button inside the Unity Editor.


## 🧩 Features

- 🧠 Automatically open C# scripts in Neovim when clicked in Unity
- 🖥️ Opens all files from the same Unity project in the **same terminal and buffer**
- 🔄 Regenerate `.csproj` files via a Unity button
- 🔌 Unity package + Neovim plugin architecture
- 🔥 Zero dependency on `nvr` (Neovim Remote)


## 📂 Installation

### 🖥️ Windows

Download the latest installer:

👉 [Download for Windows](https://github.com/apyra/nvim-unity/releases/latest/download/nvimunity-setup.exe)

---

### 🐧 Linux

✅ Test and feedbacks required!

 👉 [Download .AppImage](https://github.com/apyra/nvim-unity/releases/latest/download/nvimunity-linux.AppImage)

- **AppImage (Portable)**  

  ```bash
  mv NvimUnity.AppImage ~/.local/bin/nvimunity
  chmod +x ~/.local/bin/nvimunity

  ```

After installing, run with `nvimunity` or find it in your applications.  

---

### 🍎 macOS ###

✅ Test and feedbacks required!

👉 [Download for macOS](https://github.com/apyra/nvim-unity/releases/latest/download/NvimUnity.dmg).

#### Installation Steps

1. Open the downloaded `.dmg` file.
2. Drag the `NvimUnity.app` into your `Applications` folder.
3. Launch `NvimUnity` from your `Applications` folder.

---

* 🚀 Nice tip for the standalones: Hold **Shift** while launching to load the last Unity opened project.

### Unity

1. Install the Unity package from Git:

```json
https://github.com/apyra/nvim-unity.git
```
2. Unity will auto-detect the editor as **"Neovim Code Editor"** in `External Tools`:

- Go to `Edit > Preferences > External Tools`
- Select **Neovim Code Editor** (no need to browse for `.exe`)


### Neovim (Lua)

### 🔁 File Sync with [nvim-unity-sync](https://github.com/apyra/nvim-unity-sync)

This plugin keeps your `.csproj` updated when creating/renaming/deleting `.cs` files in nvim even if Unity is closed:

```lua
{
  "apyra/nvim-unity-sync",
  lazy = false,
  config = function()
    require("unity.plugin").setup()
  end,
}
```

## Usage

### Open Files from Unity
- Click in a `.cs` file in Unity and it will open in Neovim, simple like that.
- All files from the same project share a terminal.

### Regenerate Project Files
- From Unity: `Preferences > External Tools > Regenerate Project Files`
- From Tools Menu: `Tools > Neovim Code Editor > Regenerate Project Files`

## 🧠 Recommended Configuration

- [Neovim](https://neovim.io/) *(required)*
- [NvChad](https://nvchad.com/) as your Neovim base, with:
  - LSP  
  - Lazy.nvim  
  - Mason.nvim  
  - Telescope  
  - GitSigns  
  - Tabs for multi-file editing


### 🔧 LSP Setup for C#

Install `omnisharp` using [mason.nvim](https://github.com/williamboman/mason.nvim), it requires .Net installed.

```lua
local lspconfig = require("lspconfig")
lspconfig.omnisharp.setup {
  on_attach = nvlsp.on_attach,
  capabilities = nvlsp.capabilities,
  cmd = {
    "dotnet",
    vim.fn.stdpath "data" .. "\\mason\\packages\\omnisharp\\libexec\\OmniSharp.dll",
  },
  settings = {
    FormattingOptions = {
      EnableEditorConfigSupport = false,
      OrganizeImports = true,
    },
    Sdk = {
      IncludePrereleases = true,
    },
  },
}
```

> ⚠️ Omnisharp requires `.csproj` and `.sln` in the root folder — make sure to use "Regenerate Project Files" if missing. *It may take a second or two to attach into your working buffer.


### 🌲 Treesitter Setup 

```lua
{
  "nvim-treesitter/nvim-treesitter",
  opts = {
    ensure_installed = {
      "c_sharp",
    },
  },
}
```

Then run `:TSInstall` in Neovim.


### 🤖 Copilot (Optional)

```lua
{
  "github/copilot.vim",
  lazy = false,
  config = function()
    vim.g.copilot_no_tab_map = true
    vim.g.copilot_assume_mapped = true
  end,
}
```

In `mappings.lua` (NvChad):

```lua
map("i", "<C-l>", function()
  vim.fn.feedkeys(vim.fn["copilot#Accept"](), "")
end, { desc = "Copilot Accept", noremap = true, silent = true })
```

### 🧬 Vim Fugitive for Git (Optional)

```lua
use {
  'tpope/vim-fugitive',
  config = function()
    -- Optional config
  end
}
```

### 📁 Folding with [nvim-ufo](https://github.com/kevinhwang91/nvim-ufo) (Optional)

```lua
{
  "kevinhwang91/nvim-ufo",
  -- your config here
}
```

## 🧩 Unity Snippets

Use [LuaSnip](https://github.com/L3MON4D3/LuaSnip) to insert Unity C# boilerplate.

### ✍️ Snippet Setup

**Linux/macOS:**

```
~/.config/nvim/lua/snippets/cs.lua
```

**Windows:**

```
C:/Users/YOUR_USERNAME/AppData/Local/nvim/lua/snippets/cs.lua
```

### 🔌 Load Snippets

**Linux/macOS:**

```lua
require("luasnip.loaders.from_lua").load({ paths = "~/.config/nvim/lua/snippets" })
```

**Windows:**

```lua
require("luasnip.loaders.from_lua").lazy_load({
  paths = vim.fn.stdpath("config") .. "/lua/snippets"
})
```

### 🧾 Example Snippets

```lua
-- cs.lua
local ls = require("luasnip")
local s, t, i = ls.snippet, ls.text_node, ls.insert_node

return {
  s("start", { t("void Start() {"), t({"", "    "}), i(1), t({"", "}"}) }),
  s("update", { t("void Update() {"), t({"", "    "}), i(1), t({"", "}"}) }),
  s("awake", { t("void Awake() {"), t({"", "    "}), i(1), t({"", "}"}) }),
  s("fixedupdate", { t("void FixedUpdate() {"), t({"", "    "}), i(1), t({"", "}"}) }),
  s("onenable", { t("void OnEnable() {"), t({"", "    "}), i(1), t({"", "}"}) }),
  s("ondisable", { t("void OnDisable() {"), t({"", "    "}), i(1), t({"", "}"}) }),
  s("ontriggerenter", { t("void OnTriggerEnter(Collider other) {"), t({"", "    "}), i(1), t({"", "}"}) }),
  s("oncollisionenter", { t("void OnCollisionEnter(Collision collision) {"), t({"", "    "}), i(1), t({"", "}"}) }),
  s("serializefield", { t("[SerializeField] private "), i(1, "Type"), t(" "), i(2, "variableName"), t(";") }),
  s("publicfield", { t("public "), i(1, "Type"), t(" "), i(2, "variableName"), t(";") }),
  s("log", { t('Debug.Log("'), i(1, "message"), t('");') }),
  s("class", {
    t("using UnityEngine;"), t({"", ""}),
    t("public class "), i(1, "ClassName"), t(" : MonoBehaviour"),
    t({"", "{"}),
    t({"", "    "}), i(2, "// Your code here"),
    t({"", "}"}),
  }),
}
```

## 🛠️ Known Issues

When you create, delete, or move scripts in Unity, synchronization with the default code editor only happens after the compilation process. So if you open a script before it has been included in the .csproj, you won’t have LSP goodies like code completion.

The fastest workflow: First open your project in unity it will automatically generates the .csproj for you. If you don't want to wait for compilation, create .cs files directly in Neovim (e.g., using nvim-tree or an LSP action like "move to its own file") with [nvim-unity-sync](https://github.com/apyra/nvim-unity-sync) installed. And if for some reason any script is not included in the project, run :Usync to sync all .cs files in the Assets folder. Once you have the .csproj already generated, you can edit your code without Unity if you wish.

## ⚡ Final Note

Neovim is a blazing-fast editor. With NvChad + LSP + this Unity integration, it becomes a powerful Unity dev environment.

## 🧑 Contributing

PRs, issues, ideas welcome! This project is evolving – help shape it 🚀

## 📄 License

MIT License).
