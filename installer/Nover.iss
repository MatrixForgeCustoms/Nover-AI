; Nover Setup - Inno Setup 6 Skript
; Wird mit build_installer.bat erzeugt.

#define MyAppName "Nover"
#define MyAppVersion "1.0.0"
#define MyAppExe "Nover.exe"
#define PublishDir "..\NoverApp\bin\Release\net8.0-windows\win-x64\publish"

[Setup]
AppId={{7B3F2C1A-4E5D-4A9B-9C8E-2F1D0A6B5C43}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher=Nover
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=Nover-Setup-{#MyAppVersion}
SetupIconFile=..\NoverApp\Assets\nover.ico
UninstallDisplayIcon={app}\{#MyAppExe}
UninstallDisplayName={#MyAppName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
ShowLanguageDialog=auto

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
german.WelcomeLabel2=Dieser Assistent installiert [name/ver] auf deinem Computer.%n%nDeine Konten, Gespräche und Tokens liegen in %APPDATA%\Nover und bleiben beim Installieren und Deinstallieren erhalten.

[Tasks]
Name: "desktopicon"; Description: "Verknüpfung auf dem Desktop erstellen"; GroupDescription: "Verknüpfungen:"; Flags: checkedonce
Name: "startmenu"; Description: "Im Startmenü anlegen"; GroupDescription: "Verknüpfungen:"; Flags: checkedonce
Name: "autostart"; Description: "Nover beim Windows-Start automatisch öffnen"; GroupDescription: "Optionen:"; Flags: unchecked
Name: "runasadmin"; Description: "Nover immer mit Administratorrechten starten"; GroupDescription: "Optionen:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\{#MyAppExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishDir}\wwwroot\*"; DestDir: "{app}\wwwroot"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\NoverApp\Assets\nover.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExe}"; IconFilename: "{app}\nover.ico"; Tasks: startmenu
Name: "{group}\{#MyAppName} deinstallieren"; Filename: "{uninstallexe}"; Tasks: startmenu
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExe}"; IconFilename: "{app}\nover.ico"; Tasks: desktopicon

[Registry]
; Autostart beim Windows-Start
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "Nover"; ValueData: """{app}\{#MyAppExe}"""; Flags: uninsdeletevalue; Tasks: autostart
; Immer mit Administratorrechten starten (Kompatibilitaetsflag des Programms)
Root: HKCU; Subkey: "Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"; ValueType: string; ValueName: "{app}\{#MyAppExe}"; ValueData: "~ RUNASADMIN"; Flags: uninsdeletevalue; Tasks: runasadmin

[Run]
Filename: "{app}\{#MyAppExe}"; Description: "Nover jetzt starten"; Flags: nowait postinstall skipifsilent

[Code]
var
  OrtSeite: TInputOptionWizardPage;

// Zusatzseite: 64-Bit-Programmordner oder x86-Programmordner
procedure InitializeWizard;
begin
  OrtSeite := CreateInputOptionPage(wpWelcome,
    T('Installationsort', 'Install location'),
    T('Wo soll Nover installiert werden?', 'Where should Nover be installed?'),
    T('Die App selbst ist 64-Bit. Der x86-Ordner ist nur eine Ablage, falls du ihn ausdrücklich brauchst.',
      'The app itself is 64-bit. The x86 folder is only an alternative location if you explicitly need it.'),
    True, False);
  OrtSeite.Add(T('Programme (64-Bit, empfohlen)  -  C:\Program Files\Nover',
                 'Program Files (64-bit, recommended)  -  C:\Program Files\Nover'));
  OrtSeite.Add(T('Programme (x86)  -  C:\Program Files (x86)\Nover',
                 'Program Files (x86)  -  C:\Program Files (x86)\Nover'));
  OrtSeite.Values[0] := True;
end;

// Ordner in der Auswahl setzen, bevor die Ordnerseite erscheint
function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID = OrtSeite.ID then
  begin
    if OrtSeite.Values[0] then
      WizardForm.DirEdit.Text := ExpandConstant('{commonpf64}\Nover')
    else
      WizardForm.DirEdit.Text := ExpandConstant('{commonpf32}\Nover');
  end;
end;

// Prueft, ob die WebView2-Laufzeit installiert ist.
function WebView2Vorhanden: Boolean;
var
  Version: String;
begin
  Result :=
    RegQueryStringValue(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version) or
    RegQueryStringValue(HKCU, 'Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version);
end;

// Texte passend zur gewaehlten Setup-Sprache (automatisch aus Windows)
function T(de, en: String): String;
begin
  if ActiveLanguage = 'german' then Result := de else Result := en;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  if not WebView2Vorhanden then
    MsgBox(T('Die Microsoft WebView2-Laufzeit fehlt. Nover braucht sie für die Oberfläche.' + #13#10 +
             'Installation unter https://developer.microsoft.com/microsoft-edge/webview2 möglich.' + #13#10 +
             'Die Installation geht trotzdem weiter.',
             'The Microsoft WebView2 Runtime is missing. Nover needs it for its user interface.' + #13#10 +
             'You can install it from https://developer.microsoft.com/microsoft-edge/webview2.' + #13#10 +
             'The installation continues anyway.'), mbInformation, MB_OK);
end;

// Deinstallation: Programm wird entfernt, Daten nur nach Rueckfrage.
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  DatenOrdner: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    DatenOrdner := ExpandConstant('{userappdata}\Nover');
    if DirExists(DatenOrdner) then
    begin
      if MsgBox(T('Sollen auch deine Konten, Gespräche und Tokens gelöscht werden?',
                  'Do you also want to delete your accounts, chats, and tokens?') + #13#10 +
                '(' + DatenOrdner + ')' + #13#10#13#10 +
                T('Nein = Daten bleiben erhalten, falls du Nover später wieder installierst.',
                  'No = data is kept in case you reinstall Nover later.'),
                mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then
        DelTree(DatenOrdner, True, True, True);
    end;
  end;
end;
