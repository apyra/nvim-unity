@echo off
setlocal

set FILE=%1
set LINE=%2

:: fallback da linha
if "%LINE%"=="" (
    set LINE=1
)

:: Verifica se nvr está disponível
where nvr >nul 2>nul
if %errorlevel%==0 (
    echo [nvim-open] Abrindo com nvr
    nvr --remote-tab "%FILE%" +%LINE%
) else (
    echo [nvim-open] Abrindo com nvim local
    nvim --listen \\.\pipe\nvim-pipe "%FILE%" +%LINE%
)

