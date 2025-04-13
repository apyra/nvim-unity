@echo off
setlocal enabledelayedexpansion

set "TERMINAL="
set "ARGS="

for %%A in (%*) do (
    if not defined TERMINAL (
        set "TERMINAL=%%A"
    ) else (
        set "ARGS=!ARGS! %%A"
    )
)

set "ARGS=%ARGS:~1%"

if %TERMINAL%=="wt" (
    %TERMINAL% cmd /c "nvim %ARGS%"
) else (
    %TERMINAL% /c "nvim %ARGS%"
)

endlocal
