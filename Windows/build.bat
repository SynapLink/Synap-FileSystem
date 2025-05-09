@echo off
echo Building Synap Editor for Windows...

rem check for dotnet
where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [ERROR] .NET SDK not found
    echo install from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

rem verify installed ver
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo Using .NET SDK version %DOTNET_VERSION%

rem cleanup old build
if exist "SynapEditor\bin" rmdir /s /q "SynapEditor\bin"
if exist "SynapEditor\obj" rmdir /s /q "SynapEditor\obj"

rem restore deps
echo Restoring dependencies...
dotnet restore SynapEditor

rem build for prod
echo Building app...
dotnet publish SynapEditor -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true

echo Build completed
echo Executable is located at: SynapEditor\bin\Release\net6.0-windows\win-x64\publish\SynapEditor.exe

REM Ask user if they want to run the application
set /p runapp="Do you want to launch Synap Editor now? (Y/N): "
if /i "%runapp%"=="Y" (
    echo Starting Synap Editor...
    start "" "bin\Release\net6.0-windows\win-x64\publish\SynapEditor.exe"
)

pause 