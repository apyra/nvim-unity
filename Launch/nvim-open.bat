@echo off
setlocal EnableDelayedExpansion

:: Caminho do arquivo
set "FILE=%~1"
set "LINE=%~2"

:: Monta o caminho com linha (se houver)
if defined LINE (
    set "FILE_LINE=%FILE%+%LINE%"
) else (
    set "FILE_LINE=%FILE%"
)

:: Envia requisição HTTP para o servidor local
echo !FILE_LINE! | curl -s -X POST http://localhost:5005/open --data-binary @-




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

