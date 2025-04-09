@echo off
setlocal

set FILE=%1
set LINE=%2
set SOCKET=%3
set ROOT=%4
set ISSERVER=%5

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
    if "%ISSERVER%"(
   %TERMINAL% cmd /c "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
   )else (
    %TERMINAL% cmd /c "nvim --server \"%SOCKET%\" --remote-send \":e '%FILE%'<CR>'%LINE%'G\""
   )
) else (
    if "%ISSERVER%"(
   %TERMINAL% /c "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
   )else (
    %TERMINAL% /c "nvim --server \"%SOCKET%\" --remote-send \":e '%FILE%'<CR>'%LINE%'G\""
   )
   
)


