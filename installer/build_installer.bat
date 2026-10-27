@echo off
REM Builds Nover.exe (single file) and then the setup wizard.
REM Requires: .NET 8 SDK and Inno Setup 6 (https://jrsoftware.org/isdl.php)

cd /d "%~dp0\..\NoverApp"

echo [1/2] Building Nover.exe ...
dotnet publish NoverApp.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
if errorlevel 1 (
    echo Build of the app failed.
    pause
    exit /b 1
)

echo [2/2] Building the setup ...
set ISCC="%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if not exist %ISCC% set ISCC="%ProgramFiles%\Inno Setup 6\ISCC.exe"
if not exist %ISCC% set ISCC="%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe"
if not exist %ISCC% (
    echo Inno Setup 6 was not found. Please install it.
    pause
    exit /b 1
)
cd /d "%~dp0"
%ISCC% Nover.iss
if errorlevel 1 (
    echo Building the setup failed.
    pause
    exit /b 1
)

echo Done. Setup is in dist\Nover-Setup-1.0.0.exe
pause
