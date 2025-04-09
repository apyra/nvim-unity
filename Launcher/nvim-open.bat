@echo off
setlocal

set FILE=%1
set LINE=%2
set SOCKET=%3
set ROOT=%4

set SCRIPT_DIR=%~dp0
set CONFIG_FILE=%SCRIPT_DIR%config.json

rem Lê terminal
for /f "tokens=2 delims=:" %%a in ('findstr /C:"Windows" "%CONFIG_FILE%"') do (
    set "TERMINAL=%%~a"
)

set TERMINAL=%TERMINAL:"=%  
set TERMINAL=%TERMINAL:,=%  
set TERMINAL=%TERMINAL: =%


if /i "%TERMINAL%"=="wt" (
   %TERMINAL% cmd /c "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
) else (
   %TERMINAL% /c "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
)


