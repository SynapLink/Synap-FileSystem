@echo off
echo.
echo ===================================
echo        Synap Editor Console
echo ===================================
echo.

set EXEPATH=SynapEditor\bin\Release\net6.0-windows\win-x64\publish\SynapEditor.exe

if not exist "%EXEPATH%" (
    echo [ERROR] Synap Editor executable not found!
    echo.
    echo Please build the application first by running build.bat
    echo.
    pause
    exit /b 1
)

echo [INFO] Starting Synap Editor...
echo [INFO] Press Ctrl+C to terminate the application if it doesn't close properly.
echo.

start "" "%EXEPATH%" 