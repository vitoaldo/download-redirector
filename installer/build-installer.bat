@echo off
REM ==========================================================================
REM Build script para o Inno Setup - chamado pelo GitHub Actions
REM Uso: build-installer.bat <versao>
REM Exemplo: build-installer.bat 1.0.0
REM ==========================================================================

set VERSION=%1
if "%VERSION%"=="" (
    echo ERRO: Versao nao informada. Uso: build-installer.bat 1.0.0
    exit /b 1
)

echo === Buscando ISCC.exe ===
set "ISCC="
for /d %%D in ("C:\Program Files (x86)\Inno Setup*") do (
    if exist "%%D\ISCC.exe" set "ISCC=%%D\ISCC.exe"
)

if "%ISCC%"=="" (
    echo ERRO: ISCC.exe nao encontrado em Program Files x86
    exit /b 1
)

echo Encontrado: %ISCC%
echo Versao: %VERSION%
echo Diretorio: %CD%

echo === Listando publish\ ===
dir publish\ /b 2>nul || echo AVISO: pasta publish nao encontrada

echo === Executando Inno Setup ===
"%ISCC%" "/DMyAppVersion=%VERSION%" "installer\installer.iss"

if %ERRORLEVEL% neq 0 (
    echo ERRO: Inno Setup falhou com codigo %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

echo === Instalador gerado com sucesso ===
dir installer\Output\ /b
