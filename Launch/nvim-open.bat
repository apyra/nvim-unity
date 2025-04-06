@echo off
set FILE=%1
set LINE=%2

:: Prefer nvr if available
where nvr >nul 2>nul
if %errorlevel%==0 (
    nvr --remote-tab "%FILE%" %LINE%
) else (
    start "" nvim "%FILE%" %LINE%
)


