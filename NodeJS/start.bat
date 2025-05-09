@echo off
echo Starting Synap Editor NodeJS version...

rem check for electron
where npx >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo electron not found
    echo install with: npm install electron -g
    pause
    exit /b 1
)

rem check for deps
if not exist "node_modules" (
    echo installing dependencies
    npm install
)

echo starting app
npx electron .

pause 