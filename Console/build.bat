@echo off
echo Building Synap Editor Console version...

rem check for dotnet
where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [ERROR] .NET SDK not found
    echo install from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

rem show sdk version
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo Using .NET SDK version %DOTNET_VERSION%

rem go to project dir
cd SynapEditor

rem restore deps
echo Restoring dependencies...
dotnet restore

rem build for prod
echo Building app...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true

rem check build success
if %ERRORLEVEL% neq 0 (
    echo [ERROR] build failed
    pause
    exit /b 1
)

echo [INFO] build successful
echo Executable at: SynapEditor\bin\Release\net6.0\win-x64\publish\SynapEditor.exe

rem ask to run
echo.
set /p run_now="Run the application now? (y/n): "
if /i "%run_now%"=="y" (
    SynapEditor\bin\Release\net6.0\win-x64\publish\SynapEditor.exe
)

rem ask for shortcut
set /p create_shortcut="Create desktop shortcut? (y/n): "
if /i "%create_shortcut%"=="y" (
    echo Creating desktop shortcut...
    
    rem create temp vbs script to make shortcut
    echo Set oWS = WScript.CreateObject("WScript.Shell") > CreateShortcut.vbs
    echo sLinkFile = oWS.SpecialFolders("Desktop") ^& "\Synap Editor Console.lnk" >> CreateShortcut.vbs
    echo Set oLink = oWS.CreateShortcut(sLinkFile) >> CreateShortcut.vbs
    echo oLink.TargetPath = "%CD%\SynapEditor\bin\Release\net6.0\win-x64\publish\SynapEditor.exe" >> CreateShortcut.vbs
    echo oLink.Description = "Synap Editor Console Version" >> CreateShortcut.vbs
    echo oLink.Save >> CreateShortcut.vbs
    
    rem run script and delete it
    cscript CreateShortcut.vbs
    del CreateShortcut.vbs
    
    echo Shortcut created on desktop
)

cd ..
pause 