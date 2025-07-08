using System;

namespace MarkdownViewer
{
    public class ArgumentParser
    {
        public string? FilePath { get; set; }
        public bool BrowserMode { get; set; }
        public bool FullscreenMode { get; set; }
        
        public static ArgumentParser Parse(string[] args)
        {
            var parser = new ArgumentParser();
            
            for (int i = 1; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "--browser":
                    case "-b":
                        parser.BrowserMode = true;
                        break;
                    case "--app":
                    case "-a":
                        parser.BrowserMode = false;
                        break;
                    case "--fullscreen":
                    case "-f":
                        parser.FullscreenMode = true;
                        break;
                    default:
                        if (!arg.StartsWith("-"))
                        {
                            parser.FilePath = arg;
                        }
                        break;
                }
            }
            
            return parser;
        }
        
        public string GetUsageText()
        {
            return @"# Markdown Viewer

## Usage

```
mdv <file.md> [options]
```

## Options

- `--browser` or `-b`: Open in browser mode
- `--app` or `-a`: Open in app mode (default)
- `--fullscreen` or `-f`: Start in fullscreen mode

## Keyboard Shortcuts

- **ESC**: Quit (or exit fullscreen)
- **Ctrl+B**: Toggle between app and browser mode
- **F11**: Toggle fullscreen mode
- **Ctrl+C**: Copy as plain text
- **Ctrl+Shift+C**: Copy as rich text
- **Ctrl+Alt+C**: Copy as HTML
- **Ctrl+M**: Copy as markdown

## Examples

```
mdv README.md
mdv doc.md --browser
mdv notes.md -b
mdv presentation.md --fullscreen
```";
        }
    }
}