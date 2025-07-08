# Feature Roadmap for mdv

## Current Features
- ✅ In-app markdown rendering with Avalonia controls
- ✅ Browser-based rendering with JavaScript libraries (Marked.js, Mermaid, Prism.js)
- ✅ Command-line mode switching (`--browser`, `--app`)
- ✅ Runtime mode switching (Ctrl+B)
- ✅ Mermaid diagram detection with browser preview
- ✅ Cross-platform support (Windows, macOS, Linux)

## Planned Features

### 1. Fullscreen Mode [Issue #1]
**Description:** Add borderless fullscreen mode for distraction-free reading
- **Trigger:** F11 key or command-line flag `--fullscreen`
- **UI:** Light overlay with "Press ESC to close" message
- **Auto-hide:** Overlay disappears after 3 seconds, reappears on mouse movement
- **Implementation:** Window state management with overlay control

### 2. Content Copying [Issue #2]
**Description:** Multiple copy formats for enhanced productivity
- **Formats:**
  - Plain text (Ctrl+C)
  - Rich text (Ctrl+Shift+C)
  - HTML (Ctrl+Alt+C)
  - Original markdown (Ctrl+M)
- **Context menu:** Right-click for copy options
- **Implementation:** Clipboard API integration with format conversion

### 3. Display Mode Persistence [Issue #3]
**Description:** Remember user's preferred display mode
- **Storage:** User preferences in config file
- **Scope:** Per-user settings
- **Options:** Default mode, window size, fullscreen preference

### 4. Enhanced Navigation [Issue #4]
**Description:** Better document navigation and viewing
- **Features:**
  - Table of contents generation
  - Jump to heading (Ctrl+G)
  - Document outline in sidebar
  - Search within document (Ctrl+F)

### 5. Theme Support [Issue #5]
**Description:** Dark mode and custom themes
- **Modes:** Light, Dark, System preference
- **Customization:** User-defined CSS themes for browser mode
- **Toggle:** Ctrl+T for theme switching

## Testing Strategy

### TDD Implementation Areas
1. **Command-line argument parsing** - Unit tests for all flag combinations
2. **Content format conversion** - Test markdown → plain text, HTML, rich text
3. **Configuration management** - Test settings persistence and loading
4. **Keyboard shortcut handling** - Test all key combinations and conflicts

### Integration Tests
1. **Cross-platform browser opening** - Test on Windows, macOS, Linux
2. **File format support** - Test various markdown files and edge cases
3. **Mode switching** - Test seamless transitions between app/browser modes

### Manual Testing
1. **UI responsiveness** - Fullscreen mode, overlay behavior
2. **Accessibility** - Screen reader compatibility, keyboard navigation
3. **Performance** - Large file handling, memory usage