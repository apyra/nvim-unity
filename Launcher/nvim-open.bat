@echo off
setlocal

set "FILE=%~1"
set "LINE=%~2"
set "SOCKET=%~3"
set "ROOT=%~4"
set "ISPROJECTOPEN=%~5"

@echo on
echo FILE: %1
echo LINE: %2
echo SOCKET: %3
echo ROOT: %4
echo ISPROJECTOPEN: %5
pause

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
        echo Neovim já aberto - enviando comando
        %TERMINAL% cmd /k "nvim --server \"%SOCKET%\" --remote-send \":e '%FILE%'<CR>'%LINE%'G\""
    ) else (
        echo Neovim ainda não aberto - iniciando com --listen
        %TERMINAL% cmd /k "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
    )
) else (
    if /i "%ISPROJECTOPEN%"=="true" (
        echo Neovim já aberto - enviando comando
        %TERMINAL% /k "nvim --server \"%SOCKET%\" --remote-send \":e '%FILE%'<CR>'%LINE%'G\""
    ) else (
        echo Neovim ainda não aberto - iniciando com --listen
        %TERMINAL% /k "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
    )
)

