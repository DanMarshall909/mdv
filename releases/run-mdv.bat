@echo off
REM Simple launcher for mdv
REM Usage: run-mdv.bat yourfile.md

if "%1"=="" (
    echo Usage: run-mdv.bat [markdown-file]
    echo Example: run-mdv.bat README.md
    pause
    exit /b 1
)

REM Try to run the single-file executable first
if exist mdv-win-x64.exe (
    mdv-win-x64.exe %*
    exit /b %errorlevel%
)

REM If that fails, try the distribution version
if exist mdv-win-x64\mdv.exe (
    mdv-win-x64\mdv.exe %*
    exit /b %errorlevel%
)

REM If neither exists, show error
echo Error: mdv executable not found.
echo Please make sure mdv-win-x64.exe or mdv-win-x64\mdv.exe exists in this directory.
pause
exit /b 1