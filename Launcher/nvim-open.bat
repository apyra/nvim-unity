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

rem ESC para CR
for /f %%C in ('echo prompt $E ^| cmd') do set "ESC=%%C"

set "CMD1=:e %FILE%%ESC%"
set "CMD2=%LINE%G%ESC%"

echo.
echo ===== Enviando para Neovim =====
echo Arquivo: %FILE%
echo Linha: %LINE%
echo Socket: %SOCKET%
echo Comando 1: %CMD1%
echo Comando 2: %CMD2%
echo =================================
echo.

if /i "%TERMINAL%"=="wt" (
    if /i "%ISPROJECTOPEN%"=="true" (
        %TERMINAL% cmd /k ^
            "nvim --server \"%SOCKET%\" --remote-send \"%CMD1%\" && nvim --server \"%SOCKET%\" --remote-send \"%CMD2%\""
    ) else (
        %TERMINAL% cmd /k ^
            "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
    )
) else (
    if /i "%ISPROJECTOPEN%"=="true" (
        %TERMINAL% /k ^
            "nvim --server \"%SOCKET%\" --remote-send \"%CMD1%\" && nvim --server \"%SOCKET%\" --remote-send \"%CMD2%\""
    ) else (
        %TERMINAL% /k ^
            "cd /d \"%ROOT%\" && nvim --listen \"%SOCKET%\" \"%FILE%\" +%LINE%"
    )
)

pause

