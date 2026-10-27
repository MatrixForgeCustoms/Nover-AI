@echo off
setlocal
title Nover Uninstall Wizard

echo ==========================================
echo            Nover Uninstall Wizard
echo ==========================================
echo.
echo This removes Nover, its shortcuts and its autostart entry.
echo Your accounts and chats are only deleted if you say so in the
echo uninstaller window.
echo.
set /p "ANTWORT=Continue? (Y/N): "
if /i not "%ANTWORT%"=="Y" (
    echo Cancelled. Nothing was changed.
    pause
    exit /b 0
)

echo.
echo [1/5] Closing Nover if it is running ...
taskkill /IM Nover.exe /F >nul 2>&1

echo [2/5] Starting the uninstaller ...
set "UNINST="
if exist "%ProgramFiles%\Nover\unins000.exe" set "UNINST=%ProgramFiles%\Nover\unins000.exe"
if not defined UNINST if exist "%ProgramFiles(x86)%\Nover\unins000.exe" set "UNINST=%ProgramFiles(x86)%\Nover\unins000.exe"
if defined UNINST (
    start /wait "" "%UNINST%"
) else (
    echo    No uninstaller found. Continuing with manual cleanup.
)

echo [3/5] Removing shortcuts ...
del /q "%USERPROFILE%\Desktop\Nover.lnk" >nul 2>&1
del /q "%PUBLIC%\Desktop\Nover.lnk" >nul 2>&1
rd /s /q "%ProgramData%\Microsoft\Windows\Start Menu\Programs\Nover" >nul 2>&1
rd /s /q "%APPDATA%\Microsoft\Windows\Start Menu\Programs\Nover" >nul 2>&1

echo [4/5] Removing autostart and admin setting ...
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v Nover /f >nul 2>&1
reg delete "HKCU\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers" /v "%ProgramFiles%\Nover\Nover.exe" /f >nul 2>&1

echo [5/5] Checking the program folder ...
if exist "%ProgramFiles%\Nover" (
    set /p "ORDNER=Remove the remaining folder %ProgramFiles%\Nover ? (Y/N): "
    if /i "!ORDNER!"=="Y" rd /s /q "%ProgramFiles%\Nover" >nul 2>&1
)

echo.
echo ==========================================
echo  Done. Nover has been removed.
echo  Data folder (kept unless deleted above):
echo  %APPDATA%\Nover
echo ==========================================
echo.
pause
endlocal
