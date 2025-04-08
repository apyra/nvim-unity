@echo off
setlocal EnableDelayedExpansion

:: Args: file, line, server address
set FILE=%~1
set LINE=%~2
set SERVER=%~3

:: Caminho para config.json na mesma pasta deste .bat
set "SCRIPT_DIR=%~dp0"
set "CONFIG_FILE=%SCRIPT_DIR%config.json"

:: Lê terminal configurado para Windows
for /f "tokens=2 delims=:" %%T in ('findstr /C:"\"Windows\"" "%CONFIG_FILE%"') do (
    set "TERMINAL=%%T"
)

:: Limpa espaços e aspas
set "TERMINAL=%TERMINAL:~1,-1%"
set "TERMINAL=%TERMINAL:"=%"

:: Verifica se o servidor está online
curl -s --max-time 1 %SERVER%status >nul
if errorlevel 1 (
    echo [nvim-open] Servidor não encontrado, abrindo diretamente com terminal configurado...
    %TERMINAL% nvim "%FILE%" +%LINE%
    exit /b
)

:: Envia requisição ao servidor de forma assíncrona
echo [nvim-open] Enviando para o servidor %SERVER%open
start "" curl -s -X POST %SERVER%open -H "Content-Type: text/plain" --data "%FILE%:%LINE%" >nul 2>&1

exit /b




