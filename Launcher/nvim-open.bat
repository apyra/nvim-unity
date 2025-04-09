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
set "GOTO_LINE=:%%LINE%<CR>"

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

