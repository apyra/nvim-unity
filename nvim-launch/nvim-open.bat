@echo off
set FILE=%1

:: Check if nvr is available
where nvr >nul 2>nul
if %errorlevel%==0 (
    nvr --remote-tab "%FILE%"
) else (
    nvim --listen \\.\pipe\nvim-pipe "%FILE%"
)

