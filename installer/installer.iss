; =============================================================================
; Download Redirector - Inno Setup Installer Script
; =============================================================================
; Gera um instalador profissional para Windows que:
;   - Copia os binarios para Program Files
;   - Adiciona atalho na inicializacao (Startup) do Windows
;   - Preserva o appsettings.json do usuario em atualizacoes
;   - Fecha o aplicativo antes de atualizar/desinstalar
;
; Uso local:  iscc installer.iss
; Uso no CI:  iscc /DMyAppVersion=1.2.3 /DMyPublishDir=C:\path\publish /DMyLicenseFile=C:\path\LICENSE installer.iss
; =============================================================================

; -- Variaveis de compilacao (sobrescritas pelo CI via /D) --------------------
#ifndef MyAppVersion
  #define MyAppVersion "1.2.0"
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

[Icons]
; Cria um atalho no Menu Iniciar
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
; Cria um atalho na pasta Inicializar (Startup) para iniciar com o Windows
Name: "{autostartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
; Inicia o aplicativo logo apos a instalacao
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Funcao para matar o processo antes de instalar ou desinstalar para evitar bloqueio de arquivo
procedure KillAppProcess;
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM {#MyAppExeName} /T', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Sleep(1000);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    KillAppProcess();
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    KillAppProcess();
  end;
end;
