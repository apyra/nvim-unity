@echo off
setlocal

set FILE=%1
set LINE=%2
set SOCKET=%3
set ROOT=%4
set ISPROJECTOPEN=%5

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
    if "%ISPROJECTOPEN%"(
        %TERMINAL% cmd /k "nvim --server \"%SOCKET%\" --remote-send \":e '%FILE%'<CR>'%LINE%'G\""
    )else (
        %TERMINAL% cmd /k "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
    )
) else (
    if "%ISPROJECTOPEN%"(
        %TERMINAL% /c "nvim --server \"%SOCKET%\" --remote-send \":e '%FILE%'<CR>'%LINE%'G\""
    )else (
        %TERMINAL% /c "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
    )
)


