@echo off
setlocal enabledelayedexpansion

set "FILE=%~1"
set "LINE=%~2"
set "SOCKET=%~3"
set "ROOT=%~4"
set "ISPROJECTOPEN=%~5"

rem DEBUG
echo === nvim-open.bat ===
echo FILE: %FILE%
echo LINE: %LINE%
echo SOCKET: %SOCKET%
echo ROOT: %ROOT%
echo ISPROJECTOPEN: %ISPROJECTOPEN%
echo =====================
echo.

rem Localiza o terminal no config.json
set "SCRIPT_DIR=%~dp0"
set "CONFIG_FILE=%SCRIPT_DIR%config.json"

for /f "tokens=2 delims=:" %%a in ('findstr /C:"Windows" "%CONFIG_FILE%"') do (
    set "TERMINAL=%%~a"
)

rem Remove aspas, vírgulas e espaços
set "TERMINAL=%TERMINAL:"=%"
set "TERMINAL=%TERMINAL:,=%"
set "TERMINAL=%TERMINAL: =%"

rem DEBUG terminal
echo Terminal selecionado: %TERMINAL%
echo.

rem Comando a ser enviado para Neovim já aberto
for /f %%C in ('echo prompt $E ^| cmd') do set "CR=%%C"
set "VIMCMD=:e %FILE%%CR%%LINE%G"


if /i "%TERMINAL%"=="wt" (
    if /i "%ISPROJECTOPEN%"=="true" (
        echo Neovim já aberto - enviando comando via --remote-send
        %TERMINAL% cmd /k "nvim --server \"%SOCKET%\" --remote-send \"%VIMCMD%\""
    ) else (
        echo Neovim ainda não aberto - iniciando com --listen
        %TERMINAL% cmd /k "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
    )
) else (
    if /i "%ISPROJECTOPEN%"=="true" (
        echo Neovim já aberto - enviando comando via --remote-send
        %TERMINAL% /k "nvim --server \"%SOCKET%\" --remote-send \"%VIMCMD%\""
    ) else (
        echo Neovim ainda não aberto - iniciando com --listen
        %TERMINAL% /k "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
    )
)

endlocal

