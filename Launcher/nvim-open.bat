@echo off
setlocal

set "FILE=%~1"
set "LINE=%~2"
set "SOCKET=%~3"
set "ROOT=%~4"
set "ISPROJECTOPEN=%~5"

rem Testa valor
if "%LINE%"=="" set "LINE=1"

rem Obtem o path do config.json
set "SCRIPT_DIR=%~dp0"
set "CONFIG_FILE=%SCRIPT_DIR%config.json"

rem Lê terminal para Windows
for /f "tokens=2 delims=:" %%a in ('findstr /C:"Windows" "%CONFIG_FILE%"') do (
    set "TERMINAL=%%~a"
)

rem Limpa caracteres indesejados
set "TERMINAL=%TERMINAL:"=%"
set "TERMINAL=%TERMINAL:,=%"
set "TERMINAL=%TERMINAL: =%"

rem ESCAPE para <CR>
for /f %%C in ('echo prompt $E ^| cmd') do set "ESC=%%C"

rem Monta comando de envio remoto
set "VIMCMD=:e %FILE%%ESC%%LINE%G"

echo.
echo ========================
echo Arquivo: %FILE%
echo Linha: %LINE%
echo Socket: %SOCKET%
echo Root:   %ROOT%
echo Projeto aberto: %ISPROJECTOPEN%
echo Terminal: %TERMINAL%
echo Comando Vim: %VIMCMD%
echo ========================
echo.

rem Abre no terminal configurado
if /i "%TERMINAL%"=="wt" (
    if /i "%ISPROJECTOPEN%"=="true" (
        echo Neovim já aberto - enviando via remote-send...
        %TERMINAL% cmd /k "nvim --server \"%SOCKET%\" --remote-send \"%VIMCMD%\""
    ) else (
        echo Neovim ainda não aberto - iniciando com --listen...
        %TERMINAL% cmd /k "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
    )
) else (
    if /i "%ISPROJECTOPEN%"=="true" (
        echo Neovim já aberto - enviando via remote-send...
        %TERMINAL% /k "nvim --server \"%SOCKET%\" --remote-send \"%VIMCMD%\""
    ) else (
        echo Neovim ainda não aberto - iniciando com --listen...
        %TERMINAL% /k "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
    )
)

pause

