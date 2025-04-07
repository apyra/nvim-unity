@echo off
setlocal

set FILE=%1
set LINE=%2
set DIR=%~dp0
set CONFIG=%DIR%config.json

:: Default terminal
set TERMINAL=wt

:: Try to get terminal from config.json
for /f "delims=" %%t in ('powershell -NoProfile -Command ^
    "if (Test-Path '%CONFIG%') { ^
        (Get-Content '%CONFIG%' | ConvertFrom-Json).terminal.windows ^
     }"') do set TERMINAL=%%t

:: Launch
%TERMINAL% nvim "%FILE%" +%LINE%

