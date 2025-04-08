@echo off
setlocal EnableDelayedExpansion

:: Args: file, line, server address
set FILE=%~1
set LINE=%~2
set SERVER=%~3

:: Caminho para config.json na mesma pasta deste .bat
set "SCRIPT_DIR=%~dp0"
@echo off
setlocal

set FILE=%1
set LINE=%2
set SERVER=%3

@echo off
setlocal

set FILE=%1
set LINE=%2
set SERVER=%3

:: Caminho do config
set SCRIPT_DIR=%~dp0
set CONFIG_FILE=%SCRIPT_DIR%config.json

:: Lê terminal para Windows
for /f "tokens=2 delims=:" %%a in ('findstr /C:"Windows" "%CONFIG_FILE%"') do (
    set "TERMINAL=%%~a"
)
set TERMINAL=%TERMINAL: =%
set TERMINAL=%TERMINAL:"=%

:: Testa conexão
curl -s --max-time 1 %SERVER%status >nul
if errorlevel 1 (
    echo [nvim-open] Servidor não encontrado, abrindo com %TERMINAL%...
    if /i "%TERMINAL%"=="wt" (
        start "" wt cmd /k "nvim \"%FILE%\" +%LINE%"
    ) else (
        start "" %TERMINAL% /k nvim "%FILE%" +%LINE%
    )
    exit /b
)

:: Envia para o servidor
start "" curl -s -X POST %SERVER%open -H "Content-Type: text/plain" --data "%FILE%:%LINE%" >nul 2>&1






