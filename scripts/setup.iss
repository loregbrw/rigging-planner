#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif

#ifndef VersionInfoVersion
  #define VersionInfoVersion "0.0.0.0"
#endif

#define AppName "Rigging Planner"
#define AppPublisher "Guindastes Ribas"
#define AppExeName "RiggingPlanner.App.exe"
#define AppId "{{1C87447F-4ECD-483F-9E64-CA4E9F336141}}"
#define PublishDir "..\artifacts\publish"

[Setup]
AppId={#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppComments=Planejamento e emissão de planos de rigging
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
DisableDirPage=no
DisableProgramGroupPage=no
DisableReadyPage=no
DisableWelcomePage=no
OutputDir=..\artifacts
OutputBaseFilename=RiggingPlanner-{#AppVersion}-win-x64-setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
UninstallDisplayName={#AppName} {#AppVersion}
UninstallDisplayIcon={app}\{#AppExeName}
MinVersion=10.0.17763
ArchitecturesAllowed=x64os
ArchitecturesInstallIn64BitMode=x64os
VersionInfoCompany={#AppPublisher}
VersionInfoDescription=Instalador do Rigging Planner
VersionInfoProductName={#AppName}
VersionInfoProductVersion={#AppVersion}
VersionInfoVersion={#VersionInfoVersion}

[Languages]
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na área de trabalho"; GroupDescription: "Atalhos:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs; Excludes: "*.pdb"

[Icons]
Name: "{autoprograms}\{#AppName}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autoprograms}\{#AppName}\Desinstalar {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Abrir o Rigging Planner agora"; Flags: nowait postinstall skipifsilent unchecked

[Code]
var
  InfoPage: TWizardPage;

procedure InitializeWizard;
var
  Memo: TMemo;
begin
  InfoPage := CreateCustomPage(wpWelcome, 'Antes de instalar', 'Informações sobre a instalação');
  Memo := TMemo.Create(InfoPage);
  Memo.Parent := InfoPage.Surface;
  Memo.Align := alClient;
  Memo.ReadOnly := True;
  Memo.ScrollBars := ssVertical;
  Memo.Lines.Add('O assistente vai pedir a pasta de instalação e confirmar a instalação antes de começar.');
  Memo.Lines.Add('');
  Memo.Lines.Add('O aplicativo é instalado com todos os seus componentes (inclui .NET, SQLite e Windows App SDK).');
  Memo.Lines.Add('Requisito: Microsoft Edge WebView2 Runtime, já presente no Windows 11 e na maioria dos Windows 10 atualizados.');
  Memo.Lines.Add('');
  Memo.Lines.Add('SEUS DADOS NÃO FICAM NA PASTA DE INSTALAÇÃO:');
  Memo.Lines.Add('  Banco de planos: %LOCALAPPDATA%\RiggingPlanner');
  Memo.Lines.Add('  Anexos: pasta de dados do aplicativo no perfil do usuário');
  Memo.Lines.Add('');
  Memo.Lines.Add('Atualizar: execute o instalador da versão nova sobre a instalação atual; planos e anexos são preservados.');
  Memo.Lines.Add('Desinstalar: remove apenas o aplicativo; os planos e anexos permanecem no perfil do usuário.');
end;
