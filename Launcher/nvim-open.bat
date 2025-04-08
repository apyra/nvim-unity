@echo off
setlocal

set FILE=%1
set LINE=%2
set SERVER=%3
set ROOT=%4

set SCRIPT_DIR=%~dp0
set CONFIG_FILE=%SCRIPT_DIR%config.json

cd "%ROOT%"

for /f "tokens=2 delims=:" %%a in ('findstr /C:"Windows" "%CONFIG_FILE%"') do (
    set "TERMINAL=%%~a"
)

set TERMINAL=%TERMINAL:"=%  
set TERMINAL=%TERMINAL:,=%  
set TERMINAL=%TERMINAL: =%

curl -s --max-time 1 %SERVER%status >nul
if errorlevel 1 (
    echo [NvimUnity] Server not found, opening with %TERMINAL%...
    if /i "%TERMINAL%"=="wt" (
        start "" wt cmd /c "nvim -c \"let g:unity_server = '%SERVER%'\" \"%FILE%\" +%LINE%"
    ) else (
        start "" %TERMINAL% /c "nvim -c \"let g:unity_server = '%SERVER%'\" \"%FILE%\" +%LINE%"
    )
    exit /b
)

start "" curl -s -X POST %SERVER%open -H "Content-Type: text/plain" --data "%FILE%:%LINE%" >nul 2>&1

