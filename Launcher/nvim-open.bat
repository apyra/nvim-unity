@echo off
setlocal

set FILE=%1
set LINE=%2
set SERVER=%3
set ROOT=%4

set SCRIPT_DIR=%~dp0
set CONFIG_FILE=%SCRIPT_DIR%config.json

rem Lê terminal
for /f "tokens=2 delims=:" %%a in ('findstr /C:"Windows" "%CONFIG_FILE%"') do (
    set "TERMINAL=%%~a"
)

rem Lê socket
for /f "tokens=2 delims=:" %%a in ('findstr /C:"socket" "%CONFIG_FILE%"') do (
    set "SOCKET=%%~a"
)

set TERMINAL=%TERMINAL:"=%  
set TERMINAL=%TERMINAL:,=%  
set TERMINAL=%TERMINAL: =%

set SOCKET=%SOCKET:"=%
set SOCKET=%SOCKET:,=%
set SOCKET=%SOCKET: =%

if /i "%TERMINAL%"=="wt" (
   %TERMINAL% cmd /c "cd /d \"%ROOT%\" && nvim --server \"%SOCKET%\" --remote \"%FILE%\" +%LINE%"
) else (
   %TERMINAL% /c "cd /d \"%ROOT%\" && nvim \"%SOCKET%\" --remote \"%FILE%\" +%LINE%"
)


REM set SOCKET=%SOCKET:"=%
REM set SOCKET=%SOCKET:,=%
REM set SOCKET=%SOCKET: =%

REM curl -s --max-time 1 %SERVER%status >nul
REM if errorlevel 1 (
REM     echo [NvimUnity] Server not found, opening with %TERMINAL%...
REM     if /i "%TERMINAL%"=="wt" (
REM         %TERMINAL% cmd /c "cd /d \"%ROOT%\" && nvim --listen %SOCKET% -c \"let g:unity_server = '%SERVER%'\" \"%FILE%\" +%LINE%"
REM     ) else (
REM         %TERMINAL% /c "cd /d \"%ROOT%\" && nvim --listen %SOCKET% -c \"let g:unity_server = '%SERVER%'\" \"%FILE%\" +%LINE%"
REM     )
REM     exit /b
REM )

REM start "" curl -s -X POST %SERVER%open -H "Content-Type: text/plain" --data "%FILE%:%LINE%" >nul 2>&1


