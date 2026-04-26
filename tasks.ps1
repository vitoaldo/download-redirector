param(
    [string]$Task = "help"
)

switch ($Task) {
    "start" {
        Write-Host "=> Iniciando o Download Redirector..." -ForegroundColor Green
        dotnet run --project download-redirector.csproj
    }
    "test" {
        Write-Host "=> Rodando testes..." -ForegroundColor Green
        dotnet test --verbosity normal
    }
    "build" {
        Write-Host "=> Compilando projeto..." -ForegroundColor Green
        dotnet build
    }
    "installer" {
        Write-Host "=> Gerando instalador (Build)..." -ForegroundColor Green
        # Aqui você pode mudar a versão conforme a release
        & .\installer\build-installer.bat "1.2.4"
    }
    "help" {
        Write-Host "=============================================" -ForegroundColor Cyan
        Write-Host "     Download Redirector - Task Runner       " -ForegroundColor Cyan
        Write-Host "=============================================" -ForegroundColor Cyan
        Write-Host "Uso: .\tasks.ps1 [comando]"
        Write-Host ""
        Write-Host "Comandos disponíveis:"
        Write-Host "  start      - Roda a aplicação (dotnet run)"
        Write-Host "  test       - Roda a suíte de testes (dotnet test)"
        Write-Host "  build      - Compila a aplicação e os testes (dotnet build)"
        Write-Host "  installer  - Gera o instalador .exe usando Inno Setup local"
        Write-Host "=============================================" -ForegroundColor Cyan
    }
    default {
        Write-Host "Comando desconhecido: $Task" -ForegroundColor Red
        & .\tasks.ps1 help
    }
}
