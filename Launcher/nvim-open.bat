@echo off
setlocal

set "FILE=%~1"
set "LINE=%~2"
set "SOCKET=%~3"
set "ROOT=%~4"
set "ISPROJECTOPEN=%~5"

if "%LINE%"=="" set "LINE=1"

set "SCRIPT_DIR=%~dp0"
set "CONFIG_FILE=%SCRIPT_DIR%config.json"

for /f "tokens=2 delims=:" %%a in ('findstr /C:"Windows" "%CONFIG_FILE%"') do (
    set "TERMINAL=%%~a"
)

set "TERMINAL=%TERMINAL:"=%"
set "TERMINAL=%TERMINAL:,=%"
set "TERMINAL=%TERMINAL: =%"

rem ESC para <CR>
for /f %%C in ('echo prompt $E ^| cmd') do set "ESC=%%C"

set "GOTO_LINE=%LINE%G%ESC%"

if /i "%TERMINAL%"=="wt" (
    if /i "%ISPROJECTOPEN%"=="true" (
        %TERMINAL% cmd /c ^
            "nvim --server \"%SOCKET%\" --remote \"%FILE%\" && nvim --server \"%SOCKET%\" --remote-send \"%GOTO_LINE%\""
    ) else (
        %TERMINAL% cmd /c ^
            "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
    )
) else (
    if /i "%ISPROJECTOPEN%"=="true" (
        %TERMINAL% /c ^
            "nvim --server \"%SOCKET%\" --remote \"%FILE%\" && nvim --server \"%SOCKET%\" --remote-send \"%GOTO_LINE%\""
    ) else (
        %TERMINAL% /c ^
            "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
    )
)

endlocal

