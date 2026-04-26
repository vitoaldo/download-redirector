@echo off
setlocal
set TASK=%~1

if "%TASK%"=="" set TASK=help

if "%TASK%"=="start" goto start
if "%TASK%"=="test" goto test
if "%TASK%"=="build" goto build
if "%TASK%"=="installer" goto installer
if "%TASK%"=="help" goto help

echo Comando desconhecido: %TASK%
goto help

:start
echo =^> Iniciando o Download Redirector...
dotnet run --project download-redirector.csproj
goto end

:test
echo =^> Rodando testes...
dotnet test --verbosity normal
goto end

:build
echo =^> Compilando projeto...
dotnet build
goto end

:installer
echo =^> Gerando instalador (Build)...
call installer\build-installer.bat 1.2.2
goto end

:help
echo =============================================
echo      Download Redirector - Task Runner       
echo =============================================
echo Uso: tasks.bat [comando]
echo.
echo Comandos disponiveis:
echo   start      - Roda a aplicacao (dotnet run)
echo   test       - Roda a suite de testes (dotnet test)
echo   build      - Compila a aplicacao e os testes (dotnet build)
echo   installer  - Gera o instalador .exe usando Inno Setup local
echo =============================================
goto end

:end
endlocal
