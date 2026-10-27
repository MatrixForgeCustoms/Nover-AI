; Nover Setup - Inno Setup 6
; Wizard: 1 Language | 2 Install location | 3 Terms (accept/decline) | 4 AI training (yes/no) | 5 Options | 6 Install and finish

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
ShowLanguageDialog=yes

[Languages]
; Step 1: Language. Terms are shown in the chosen language.
Name: "german"; MessagesFile: "compiler:Languages\German.isl"; LicenseFile: "terms_de.txt"
Name: "english"; MessagesFile: "compiler:Default.isl"; LicenseFile: "terms_en.txt"

[Messages]
german.WelcomeLabel2=Dieser Assistent installiert [name/ver] auf deinem Computer.%n%nDeine Konten, Gespräche und Tokens liegen in %APPDATA%\Nover und bleiben beim Installieren und Deinstallieren erhalten.

[Tasks]
; Step 5: Options
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
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "Nover"; ValueData: """{app}\{#MyAppExe}"""; Flags: uninsdeletevalue; Tasks: autostart
Root: HKCU; Subkey: "Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"; ValueType: string; ValueName: "{app}\{#MyAppExe}"; ValueData: "~ RUNASADMIN"; Flags: uninsdeletevalue; Tasks: runasadmin

[Run]
; Step 6: Finish and start
Filename: "{app}\{#MyAppExe}"; Description: "Nover jetzt starten"; Flags: nowait postinstall skipifsilent

[Code]
var
  OrtSeite: TInputOptionWizardPage;
  KiSeite: TInputOptionWizardPage;

// Texte je nach Sprache
function T(de, en: String): String;
begin
  if ActiveLanguage = 'german' then Result := de else Result := en;
end;

procedure InitializeWizard;
begin
  // Step 2: Install location (after Welcome)
  OrtSeite := CreateInputOptionPage(wpWelcome,
    T('Installationsort', 'Install location'),
    T('Wo soll Nover installiert werden?', 'Where should Nover be installed?'),
    T('Die App selbst ist 64-Bit. Wähle einen Ordner oder "Anderer Ordner" für eine eigene Auswahl.',
      'The app itself is 64-bit. Choose a folder, or "Other folder" to pick your own.'),
    True, False);
  OrtSeite.Add(T('Programme (64-Bit, empfohlen)  -  C:\Program Files\Nover',
                 'Program Files (64-bit, recommended)  -  C:\Program Files\Nover'));
  OrtSeite.Add(T('Programme (x86)  -  C:\Program Files (x86)\Nover',
                 'Program Files (x86)  -  C:\Program Files (x86)\Nover'));
  OrtSeite.Add(T('Anderer Ordner (im nächsten Schritt wählen)',
                 'Other folder (choose on the next page)'));
  OrtSeite.Values[0] := True;

  // Step 4: AI training consent (after Terms)
  KiSeite := CreateInputOptionPage(wpLicense,
    T('KI-Verbesserung', 'AI improvement'),
    T('Darf Nover deine Eingaben nutzen, um die KI zu verbessern?',
      'May Nover use your inputs to improve the AI?'),
    T('Du kannst das später ändern. Deine Entscheidung wird gespeichert.',
      'You can change this later. Your choice is saved.'),
    True, False);
  KiSeite.Add(T('Ja, meine Eingaben dürfen zur Verbesserung genutzt werden',
                'Yes, my inputs may be used to improve the AI'));
  KiSeite.Add(T('Nein, meine Eingaben nicht verwenden',
                'No, do not use my inputs'));
  KiSeite.Values[1] := True;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID = OrtSeite.ID then
  begin
    if OrtSeite.Values[0] then
      WizardForm.DirEdit.Text := ExpandConstant('{commonpf64}\Nover')
    else if OrtSeite.Values[1] then
      WizardForm.DirEdit.Text := ExpandConstant('{commonpf32}\Nover');
    // Option 3: Ordner auf der Ordner-Seite waehlen
  end;
end;

// Speichert die KI-Entscheidung in der Registry
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    if KiSeite.Values[0] then
      RegWriteStringValue(HKCU, 'Software\Nover', 'AiTrainingConsent', 'yes')
    else
      RegWriteStringValue(HKCU, 'Software\Nover', 'AiTrainingConsent', 'no');
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

// Deinstallation: Daten nur nach Rueckfrage loeschen
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
