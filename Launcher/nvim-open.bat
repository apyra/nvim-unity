@echo off
setlocal

set FILE=%1
set LINE=%2
set SERVER=%3
set ROOT=%4

set SCRIPT_DIR=%~dp0
set CONFIG_FILE=%SCRIPT_DIR%config.json

rem Lê terminal
for /f "tokens=2 delims=:" %%a in ('findstr /C:"Windows" "%CONFIG_FILE%"') do (
    set "TERMINAL=%%~a"
)

rem Lê socket
for /f "tokens=2 delims=:" %%a in ('findstr /C:"socket" "%CONFIG_FILE%"') do (
    set "SOCKET=%%~a"
)

set TERMINAL=%TERMINAL:"=%  
set TERMINAL=%TERMINAL:,=%  
set TERMINAL=%TERMINAL: =%

set SOCKET=%SOCKET:"=%
set SOCKET=%SOCKET:,=%
set SOCKET=%SOCKET: =%

curl -s --max-time 1 %SERVER%status >nul
if errorlevel 1 (
    echo [NvimUnity] Server not found, opening with %TERMINAL%...
    if /i "%TERMINAL%"=="wt" (
        %TERMINAL% cmd /c "cd /d \"%ROOT%\" && nvim --listen %SOCKET% -c \"let g:unity_server = '%SERVER%'\" \"%FILE%\" +%LINE%"
    ) else (
        %TERMINAL% /c "cd /d \"%ROOT%\" && nvim --listen %SOCKET% -c \"let g:unity_server = '%SERVER%'\" \"%FILE%\" +%LINE%"
    )
    exit /b
)

start "" curl -s -X POST %SERVER%open -H "Content-Type: text/plain" --data "%FILE%:%LINE%" >nul 2>&1

