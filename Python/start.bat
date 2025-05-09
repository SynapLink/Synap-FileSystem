@echo off
echo Starting Synap Editor Python version...
echo.
echo ===================================
echo        Synap Editor Python
echo ===================================
echo.
echo Lightweight Python version of Synap Editor for Windows, Linux, and MacOS
echo.

rem check for python
where python >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Python is not installed or not in PATH
    echo download from https://www.python.org/downloads/
    pause
    exit /b 1
)

rem verify python version
python -c "import sys; sys.exit(0 if sys.version_info >= (3, 8) else 1)"
if %ERRORLEVEL% neq 0 (
    echo [ERROR] need python 3.8+
    pause
    exit /b 1
)

rem check tkinter
python -c "import tkinter" >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [ERROR] tkinter not found
    echo reinstall python with tcl/tk option
    pause
    exit /b 1
)

rem install deps
echo [INFO] installing packages
python -m pip install -r requirements.txt --upgrade
if %ERRORLEVEL% neq 0 (
    echo [ERROR] pkg install failed
    pause
    exit /b 1
)

rem verify crypto
python -c "import cryptography" >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [ERROR] crypto import failed even after install
    echo try manual: pip install cryptography --upgrade
    pause
    exit /b 1
)

echo [INFO] all good starting app
python synap_editor.py

pause 