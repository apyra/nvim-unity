@echo off
setlocal

set "FILE=%~1"
set "LINE=%~2"
set "SOCKET=%~3"
set "ROOT=%~4"
set "ISPROJECTOPEN=%~5"

set "SCRIPT_DIR=%~dp0"
set "CONFIG_FILE=%SCRIPT_DIR%config.json"

rem Lê terminal do config.json para Windows
for /f "tokens=2 delims=:" %%a in ('findstr /C:"Windows" "%CONFIG_FILE%"') do (
    set "TERMINAL=%%~a"
)

rem Limpa caracteres indesejados
set "TERMINAL=%TERMINAL:"=%"
set "TERMINAL=%TERMINAL:,=%"
set "TERMINAL=%TERMINAL: =%"

rem Decide o comando baseado no terminal e se o projeto já está aberto
if /i "%TERMINAL%"=="wt" (
    if /i "%ISPROJECTOPEN%"=="true" (
        %TERMINAL% cmd /k "nvim --server \"%SOCKET%\" --remote-send \":e %FILE%<CR>%LINE%G\""
    ) else (
        %TERMINAL% cmd /k "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
    )
) else (
    if /i "%ISPROJECTOPEN%"=="true" (
        %TERMINAL% /k "nvim --server \"%SOCKET%\" --remote-send \":e %FILE%<CR>%LINE%G\""
    ) else (
        %TERMINAL% /k "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
    )
)


