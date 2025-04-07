@echo off
setlocal EnableDelayedExpansion

:: Caminho do arquivo
set "FILE=%~1"
set "LINE=%~2"

:: Caminho absoluto do diretório do script
set "SCRIPT_DIR=%~dp0"
set "CONFIG_FILE=%SCRIPT_DIR%config.json"

:: Valor padrão
set "TERMINAL=wt"

:: Lê o terminal do config.json
for /f "delims=" %%i in ('powershell -NoProfile -Command ^
  "if (Test-Path '%CONFIG_FILE%') { (Get-Content '%CONFIG_FILE%' -Raw | ConvertFrom-Json).terminals.Windows } else { '' }"') do (
    set "TERMINAL=%%i"
)

:: Executa o comando
%TERMINAL% new-tab nvim --listen nvim-unity -- "%FILE%" +%LINE%



REM @echo off
REM setlocal
REM
REM set FILE=%1
REM set LINE=%2
REM set DIR=%~dp0
REM set CONFIG=%DIR%config.json
REM
REM :: Default terminal
REM set TERMINAL=wt
REM
REM :: Try to get terminal from config.json
REM for /f "delims=" %%t in ('powershell -NoProfile -Command ^
REM     "if (Test-Path '%CONFIG%') { ^
REM         (Get-Content '%CONFIG%' | ConvertFrom-Json).terminal.windows ^
REM      }"') do set TERMINAL=%%t
REM
REM :: Launch
REM %TERMINAL% nvim "%FILE%" +%LINE%

