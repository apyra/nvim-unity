@echo off
setlocal

:: Args: file, line, server address
set FILE=%~1
set LINE=%~2
set SERVER=%~3

:: Testa se o servidor está online
curl -s --max-time 1 "%SERVER%status" >nul
if errorlevel 1 (
    echo [nvim-open] Servidor não encontrado, abrindo diretamente %FILE% +%LINE%
    start "" nvim "%FILE%" +%LINE%
    exit /b
)

:: Envia requisição para o servidor de forma assíncrona
echo [nvim-open] Enviando para o servidor %SERVER%open
start "" curl -s -X POST "%SERVER%open" -H "Content-Type: text/plain" --data "%FILE%:%LINE%" >nul 2>&1

exit /b



