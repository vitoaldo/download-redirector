; =============================================================================
; Download Redirector — Inno Setup Installer Script
; =============================================================================
; Gera um instalador profissional para Windows que:
;   - Copia os binários para Program Files
;   - Registra e inicia o Windows Service automaticamente
;   - Preserva o appsettings.json do usuário em atualizações
;   - Para e remove o serviço na desinstalação
;
; Uso local:  iscc installer.iss
; Uso no CI:  iscc /DMyAppVersion=1.2.3 installer.iss
; =============================================================================

; -- Variáveis de compilação (sobrescritas pelo CI via /D) --------------------
#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\publish"
#endif

; -- Metadados do aplicativo --------------------------------------------------
#define MyAppName        "Download Redirector"
#define MyAppExeName     "download-redirector.exe"
#define MyAppPublisher   "Victor Adalto Cavalcanti Valentim"
#define MyAppServiceName "DownloadRedirectorService"

[Setup]
AppId={{8F4C3B2A-1D5E-4F6A-B7C8-9D0E1F2A3B4C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\DownloadRedirector
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
OutputDir=Output
OutputBaseFilename=DownloadRedirector-Setup-{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
PrivilegesRequired=admin
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallMode=x64compatible
MinVersion=10.0
UninstallDisplayName={#MyAppName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; Binário principal (sempre sobrescreve em atualizações)
Source: "{#PublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

; Configurações do usuário (preserva personalizações em atualizações)
Source: "{#PublishDir}\appsettings.json"; DestDir: "{app}"; Flags: onlyifdoesntexist uninsneveruninstall

; Licença para referência
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[Code]
// ---------------------------------------------------------------------------
// Gerenciamento do Windows Service via sc.exe
// ---------------------------------------------------------------------------
// ssInstall:    Para e remove o serviço existente (seguro em instalação limpa)
// ssPostInstall: Cria o serviço com o caminho correto e o inicia
// Uninstall:    Para e remove o serviço antes de deletar os arquivos
// ---------------------------------------------------------------------------

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  ExePath: String;
begin
  if CurStep = ssInstall then
  begin
    // Para o serviço existente (ignora erro se não existir)
    Exec(ExpandConstant('{sys}\sc.exe'), 'stop {#MyAppServiceName}', '',
         SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(2000);

    // Remove o serviço existente para recriar com caminho atualizado
    Exec(ExpandConstant('{sys}\sc.exe'), 'delete {#MyAppServiceName}', '',
         SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(1000);
  end;

  if CurStep = ssPostInstall then
  begin
    ExePath := ExpandConstant('{app}\{#MyAppExeName}');

    // Registra o serviço
    Exec(ExpandConstant('{sys}\sc.exe'),
         'create {#MyAppServiceName} binpath= "' + ExePath + '" start= auto',
         '', SW_HIDE, ewWaitUntilTerminated, ResultCode);

    // Define descrição amigável
    Exec(ExpandConstant('{sys}\sc.exe'),
         'description {#MyAppServiceName} "Organização automática de arquivos da pasta Downloads"',
         '', SW_HIDE, ewWaitUntilTerminated, ResultCode);

    // Inicia o serviço
    Exec(ExpandConstant('{sys}\sc.exe'), 'start {#MyAppServiceName}', '',
         SW_HIDE, ewWaitUntilTerminated, ResultCode);
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ResultCode: Integer;
begin
  if CurUninstallStep = usUninstall then
  begin
    Exec(ExpandConstant('{sys}\sc.exe'), 'stop {#MyAppServiceName}', '',
         SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(3000);

    Exec(ExpandConstant('{sys}\sc.exe'), 'delete {#MyAppServiceName}', '',
         SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(1000);
  end;
end;
