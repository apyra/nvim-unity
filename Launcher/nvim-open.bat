@echo off
setlocal

set "FILE=%~1"
set "LINE=%~2"
set "TERMINAL=%~3"
set "SOCKET=%~4"
set "ROOT=%~5"
set "ISPROJECTOPEN=%~6"

rem ESC = \x1b (ASCII 27), CR = \r
set "EDIT_FILE=:e %FILE%\r"
set "GOTO_LINE=:%LINE%\r"

set "RUN_AS_SERVER=nvim --server \"%SOCKET%\" --remote-send \"%EDIT_FILE%%GOTO_LINE%\""
set "RUN_AS_CLIENT=cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"

if /i "%TERMINAL%"=="wt" (
    if /i "%ISPROJECTOPEN%"=="true" (
        call %TERMINAL% cmd /c "%RUN_AS_SERVER%"
    ) else (
        call %TERMINAL% cmd /c "%RUN_AS_CLIENT%"
    )
) else (
    if /i "%ISPROJECTOPEN%"=="true" (
        call %TERMINAL% /c "%RUN_AS_SERVER%"
    ) else (
        call %TERMINAL% /c "%RUN_AS_CLIENT%"
    )
)

endlocal

