; =============================================================================
; Download Redirector - Inno Setup Installer Script
; =============================================================================
; Gera um instalador profissional para Windows que:
;   - Copia os binarios para Program Files
;   - Registra e inicia o Windows Service automaticamente
;   - Preserva o appsettings.json do usuario em atualizacoes
;   - Para e remove o servico na desinstalacao
;
; Uso local:  iscc installer.iss
; Uso no CI:  iscc /DMyAppVersion=1.2.3 /DMyPublishDir=C:\path\publish /DMyLicenseFile=C:\path\LICENSE installer.iss
; =============================================================================

; -- Variaveis de compilacao (sobrescritas pelo CI via /D) --------------------
#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#ifndef MyPublishDir
  #define MyPublishDir "..\publish"
#endif
#ifndef MyLicenseFile
  #define MyLicenseFile "..\LICENSE"
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
LicenseFile={#MyLicenseFile}
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
; Binario principal (sempre sobrescreve em atualizacoes)
Source: "{#MyPublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

; Configuracoes do usuario (preserva personalizacoes em atualizacoes)
Source: "{#MyPublishDir}\appsettings.json"; DestDir: "{app}"; Flags: onlyifdoesntexist uninsneveruninstall

; Licenca para referencia
Source: "{#MyLicenseFile}"; DestDir: "{app}"; Flags: ignoreversion

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  ExePath: String;
begin
  if CurStep = ssInstall then
  begin
    Exec(ExpandConstant('{sys}\sc.exe'), 'stop {#MyAppServiceName}', '',
         SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(2000);
    Exec(ExpandConstant('{sys}\sc.exe'), 'delete {#MyAppServiceName}', '',
         SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(1000);
  end;

  if CurStep = ssPostInstall then
  begin
    ExePath := ExpandConstant('{app}\{#MyAppExeName}');
    Exec(ExpandConstant('{sys}\sc.exe'),
         'create {#MyAppServiceName} binpath= "' + ExePath + '" start= auto',
         '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Exec(ExpandConstant('{sys}\sc.exe'),
         'description {#MyAppServiceName} "Organizacao automatica de arquivos da pasta Downloads"',
         '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
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
