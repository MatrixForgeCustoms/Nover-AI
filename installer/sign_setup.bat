@echo off
REM Signs Nover.exe and the setup with a code signing certificate (PFX file).
REM Requirements:
REM   - Windows SDK (contains signtool.exe)
REM   - Certificate file (.pfx) and password
REM Usage:  sign_setup.bat "C:\pfad\zertifikat.pfx" "PASSWORT"
REM The password is not stored.

if "%~1"=="" (
    echo Usage: sign_setup.bat "certificate.pfx" "password"
    exit /b 1
)

set SIGNTOOL="%ProgramFiles(x86)%\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe"
if not exist %SIGNTOOL% set SIGNTOOL="%ProgramFiles(x86)%\Windows Kits\10\bin\x64\signtool.exe"
if not exist %SIGNTOOL% (
    echo signtool.exe not found. Please install the Windows SDK.
    exit /b 1
)

set ZEIT=http://timestamp.digicert.com

echo [1/2] Signing Nover.exe ...
%SIGNTOOL% sign /f "%~1" /p "%~2" /fd SHA256 /tr %ZEIT% /td SHA256 "%~dp0..\NoverApp\bin\Release\net8.0-windows\win-x64\publish\Nover.exe"
if errorlevel 1 exit /b 1

echo [2/2] Signing the setup ...
%SIGNTOOL% sign /f "%~1" /p "%~2" /fd SHA256 /tr %ZEIT% /td SHA256 "%~dp0..\dist\Nover-Setup-1.0.0.exe"
if errorlevel 1 exit /b 1

echo Done. Both files are signed.
