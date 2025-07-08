# mdv - Release Binaries

Cross-platform markdown file viewer with native HTML rendering.

## Download

| Platform | File | Size |
|----------|------|------|
| Windows x64 | `mdv-win-x64.exe` | 17MB |
| macOS x64 (Intel) | `mdv-osx-x64` | 18MB |
| macOS ARM64 (Apple Silicon) | `mdv-osx-arm64` | 17MB |
| Linux x64 | `mdv-linux-x64` | 18MB |

## Usage

```bash
# Windows
mdv-win-x64.exe README.md
mdv-win-x64.exe README

# macOS
./mdv-osx-x64 README.md
./mdv-osx-arm64 README

# Linux
./mdv-linux-x64 README.md
./mdv-linux-x64 README
```

## Installation

1. Download the appropriate binary for your platform
2. Make executable (macOS/Linux): `chmod +x mdv-*`
3. Optionally rename to `mdv` and add to PATH for global usage
4. Run with: `mdv <filename>[.md]`

## Features

- **Native HTML rendering** - Proper markdown display with styling
- **Smart file detection** - Automatically adds `.md` extension if needed
- **Cross-platform** - Single executable for each platform
- **Fast startup** - Optimized for daily use
- **ESC to quit** - Simple keyboard shortcut
- **Mermaid detection** - Identifies diagram code blocks

Perfect for daily markdown viewing from the command line!