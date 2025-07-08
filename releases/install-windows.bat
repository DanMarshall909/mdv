@echo off
echo Installing Markdown Viewer (mdv) for Windows...

REM Extract the distribution
if exist mdv-win-x64 rmdir /s /q mdv-win-x64
tar -xf mdv-win-x64.tar.gz -C mdv-win-x64 2>nul
if not exist mdv-win-x64 (
    echo Creating directory and extracting...
    mkdir mdv-win-x64
    tar -xf mdv-win-x64.tar.gz -C mdv-win-x64
)

REM Create a wrapper script
echo @echo off > mdv.bat
echo "%~dp0mdv-win-x64\mdv.exe" %%* >> mdv.bat

echo.
echo Installation complete!
echo.
echo Usage: mdv file.md
echo Example: mdv README.md
echo.
echo The mdv.bat file has been created in the current directory.
echo You can now run: mdv yourfile.md
echo.
pause