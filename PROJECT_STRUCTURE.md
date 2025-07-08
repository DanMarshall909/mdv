# Project Structure

## 📁 Directory Overview

```
mdv/
├── 📄 README.md                 # Main project documentation
├── 📄 FEATURES.md              # Feature roadmap and planned enhancements
├── 📄 PROJECT_STRUCTURE.md     # This file - project organization
├── 📄 .gitignore               # Git ignore rules
├── 📄 mdv.sln                  # Visual Studio solution file
├── 📄 install-mdv.ps1          # PowerShell installer script
├── 📄 sample.md                # Sample markdown file for testing
├── 📄 visual_test.md           # Comprehensive visual test document
│
├── 📁 MarkdownViewer/          # Main application project
│   ├── 📄 App.axaml            # Avalonia application definition
│   ├── 📄 App.axaml.cs         # Application code-behind
│   ├── 📄 MainWindow.axaml     # Main window XAML layout
│   ├── 📄 MainWindow.axaml.cs  # Main window logic and rendering
│   ├── 📄 ArgumentParser.cs    # Command-line argument parsing
│   ├── 📄 Program.cs           # Application entry point
│   ├── 📄 MarkdownViewer.csproj # Project file with dependencies
│   ├── 📄 app.manifest         # Windows application manifest
│   └── 📄 usage.md             # Usage instructions (embedded)
│
├── 📁 MarkdownViewer.Tests/    # Test project
│   ├── 📄 CommandLineTests.cs  # Unit tests for argument parsing
│   ├── 📄 VisualTests.cs       # Visual verification tests
│   └── 📄 MarkdownViewer.Tests.csproj # Test project file
│
└── 📁 releases/                # Pre-built binaries and installers
    ├── 📄 README.md            # Release documentation
    ├── 📄 install-windows.bat  # Legacy Windows installer
    ├── 📄 run-mdv.bat          # Windows launcher script
    ├── 📄 mdv-linux-x64        # Linux x64 binary
    ├── 📄 mdv-linux-x64.tar.gz # Linux distribution package
    ├── 📄 mdv-osx-x64          # macOS x64 binary
    ├── 📄 mdv-osx-x64.tar.gz   # macOS x64 distribution
    ├── 📄 mdv-osx-arm64        # macOS ARM64 binary
    ├── 📄 mdv-osx-arm64.tar.gz # macOS ARM64 distribution
    ├── 📄 mdv-win-x64.exe      # Windows standalone executable
    ├── 📄 mdv-win-x64.tar.gz   # Windows full distribution
    ├── 📄 mdv-win-x64-framework.tar.gz    # Framework-dependent
    ├── 📄 mdv-win-x64-hybrid.tar.gz       # Hybrid mode (recommended)
    └── 📄 mdv-win-x64-inapp.tar.gz        # In-app only mode
```

## 🏗️ Architecture

### **Core Components**

1. **MainWindow** - Primary UI and rendering logic
   - In-app markdown rendering with Avalonia controls
   - Browser-based rendering with JavaScript libraries
   - Fullscreen mode with overlay management
   - Keyboard shortcut handling

2. **ArgumentParser** - Command-line interface
   - Testable argument parsing logic
   - Support for multiple display modes
   - Fullscreen and browser mode flags

3. **Rendering Pipeline**
   - Markdig for markdown → HTML conversion
   - HtmlAgilityPack for HTML → Avalonia control conversion
   - JavaScript libraries for browser rendering (Marked.js, Mermaid, Prism.js)

### **Display Modes**

- **In-App Mode** (default) - Native Avalonia controls
- **Browser Mode** - Opens in system default browser
- **Hybrid Mode** - In-app with Mermaid browser preview
- **Fullscreen Mode** - Borderless with ESC overlay

### **Testing Strategy**

- **Unit Tests** - ArgumentParser logic and edge cases
- **Visual Tests** - Manual verification with reference document
- **Integration Tests** - Cross-platform functionality
- **TDD Approach** - Test-driven development for core features

## 🚀 Build & Deploy

### **Development**
```bash
dotnet build                    # Debug build
dotnet test                     # Run all tests
dotnet run --project MarkdownViewer sample.md
```

### **Release**
```bash
dotnet publish MarkdownViewer -c Release -r win-x64 --self-contained
```

### **Installation**
```powershell
.\install-mdv.ps1 -AddToPath -Test
```

## 📋 Maintenance

### **Adding New Features**
1. Write tests first (TDD approach)
2. Implement in appropriate component
3. Update documentation and visual tests
4. Test across all platforms
5. Update FEATURES.md roadmap

### **Release Process**
1. Run full test suite
2. Build all platform binaries
3. Update version numbers
4. Create GitHub release
5. Test installation scripts