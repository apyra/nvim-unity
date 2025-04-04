@echo off
REM Abrir Neovim com o arquivo passado como argumento
set FILE=%1
start wt nvim "%FILE%"