@echo off
@echo off
setlocal
set FILE=%1
set LINE=%2

:: Verifica se o servidor está rodando
powershell -Command "try { (Invoke-WebRequest -UseBasicParsing -Uri http://localhost:5005/status -TimeoutSec 1) | Out-Null; exit 0 } catch { exit 1 }"
if %errorlevel% neq 0 (
    echo [nvim-open] Servidor não encontrado, abrindo diretamente...
    start "" nvim "%FILE%" +%LINE%
    exit /b
)

:: Envia requisição para o servidor
echo %FILE%:%LINE% | curl -X POST http://localhost:5005/open -H "Content-Type: text/plain" --data-binary @-

