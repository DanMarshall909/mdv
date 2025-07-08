# Install mdv (Markdown Viewer) on Windows
# Run this script from C:\utils or specify a different installation directory

param(
    [string]$InstallDir = "C:\utils",
    [switch]$AddToPath,
    [switch]$Test
)

Write-Host "🚀 Installing mdv (Markdown Viewer)..." -ForegroundColor Cyan

# Check if running in correct directory
if (!(Test-Path $InstallDir)) {
    Write-Host "❌ Directory $InstallDir does not exist. Creating it..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
}

# Navigate to installation directory
Set-Location $InstallDir
Write-Host "📁 Working in: $InstallDir" -ForegroundColor Gray

# Check if the archive exists
$archivePath = "mdv-win-x64-hybrid.tar.gz"
if (!(Test-Path $archivePath)) {
    Write-Host "❌ Archive file '$archivePath' not found in $InstallDir" -ForegroundColor Red
    Write-Host "Please ensure the release files are copied to this directory first." -ForegroundColor Yellow
    exit 1
}

# Extract the archive
Write-Host "📦 Extracting mdv..." -ForegroundColor Green
try {
    tar -xf $archivePath
    Write-Host "✅ Extraction completed" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to extract archive: $_" -ForegroundColor Red
    exit 1
}

# Rename the extracted folder for easier access
$extractedDir = "mdv-win-x64-hybrid"
$finalDir = "mdv"

if (Test-Path $extractedDir) {
    if (Test-Path $finalDir) {
        Write-Host "⚠️  Removing existing mdv directory..." -ForegroundColor Yellow
        Remove-Item $finalDir -Recurse -Force
    }
    
    Rename-Item $extractedDir $finalDir
    Write-Host "✅ Renamed to '$finalDir'" -ForegroundColor Green
} else {
    Write-Host "❌ Extracted directory '$extractedDir' not found" -ForegroundColor Red
    exit 1
}

# Verify installation
$mdvPath = Join-Path $InstallDir $finalDir "mdv.exe"
if (Test-Path $mdvPath) {
    Write-Host "✅ mdv.exe found at: $mdvPath" -ForegroundColor Green
} else {
    Write-Host "❌ mdv.exe not found after extraction" -ForegroundColor Red
    exit 1
}

# Add to PATH if requested
if ($AddToPath) {
    $mdvDir = Join-Path $InstallDir $finalDir
    $currentPath = [Environment]::GetEnvironmentVariable("PATH", "User")
    
    if ($currentPath -notlike "*$mdvDir*") {
        Write-Host "🔧 Adding to user PATH..." -ForegroundColor Yellow
        $newPath = "$currentPath;$mdvDir"
        [Environment]::SetEnvironmentVariable("PATH", $newPath, "User")
        
        # Also add to current session
        $env:PATH += ";$mdvDir"
        Write-Host "✅ Added to PATH (restart terminal for global effect)" -ForegroundColor Green
    } else {
        Write-Host "ℹ️  Already in PATH" -ForegroundColor Gray
    }
}

# Create desktop shortcut (optional)
$shortcutPath = [Environment]::GetFolderPath("Desktop") + "\mdv.lnk"
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($shortcutPath)
$Shortcut.TargetPath = $mdvPath
$Shortcut.WorkingDirectory = [Environment]::GetFolderPath("MyDocuments")
$Shortcut.Description = "Markdown Viewer - Fast markdown file viewer"
$Shortcut.Save()
Write-Host "🔗 Desktop shortcut created" -ForegroundColor Green

# Run test if requested
if ($Test) {
    Write-Host "🧪 Running test..." -ForegroundColor Cyan
    
    # Create a test markdown file if visual_test.md doesn't exist
    $testFile = Join-Path $InstallDir "test.md"
    if (!(Test-Path "visual_test.md")) {
        $testContent = @"
# Test Installation

## Success! 🎉

mdv is now installed and working correctly.

### Features Available:

- **F11**: Fullscreen mode
- **Ctrl+B**: Toggle app/browser mode  
- **Ctrl+C**: Copy as plain text
- **Ctrl+M**: Copy as markdown

### Usage Examples:

``````
mdv README.md
mdv document.md --browser
mdv presentation.md --fullscreen
``````

Press **ESC** to close this window.
"@
        Set-Content $testFile $testContent -Encoding UTF8
        Write-Host "📝 Created test file: $testFile" -ForegroundColor Gray
        
        # Test the installation
        & $mdvPath $testFile
    } else {
        & $mdvPath "visual_test.md"
    }
}

Write-Host ""
Write-Host "🎉 Installation Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Usage:" -ForegroundColor White
Write-Host "  mdv README.md                    # Basic usage" -ForegroundColor Gray
Write-Host "  mdv document.md --browser        # Open in browser" -ForegroundColor Gray
Write-Host "  mdv presentation.md --fullscreen # Fullscreen mode" -ForegroundColor Gray
Write-Host ""
Write-Host "Keyboard Shortcuts:" -ForegroundColor White
Write-Host "  F11         - Toggle fullscreen" -ForegroundColor Gray
Write-Host "  Ctrl+B      - Toggle app/browser mode" -ForegroundColor Gray
Write-Host "  Ctrl+C      - Copy as plain text" -ForegroundColor Gray
Write-Host "  Ctrl+M      - Copy as markdown" -ForegroundColor Gray
Write-Host "  ESC         - Exit" -ForegroundColor Gray
Write-Host ""

if (!$AddToPath) {
    Write-Host "💡 Tip: Run with -AddToPath to add mdv to your PATH" -ForegroundColor Yellow
    Write-Host "   Example: .\install-mdv.ps1 -AddToPath" -ForegroundColor Gray
}

if (!$Test) {
    Write-Host "🧪 Test: Run with -Test to verify installation" -ForegroundColor Yellow
    Write-Host "   Example: .\install-mdv.ps1 -Test" -ForegroundColor Gray
}