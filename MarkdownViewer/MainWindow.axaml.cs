using Avalonia.Controls;
using Avalonia.Input;
using Markdig;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MarkdownViewer;

public partial class MainWindow : Window
{
    private readonly MarkdownPipeline _pipeline;
    private string? _tempHtmlFile;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
            
        LoadMarkdownFromArgs();
        KeyDown += OnKeyDown;
    }
    
    protected override void OnClosed(EventArgs e)
    {
        // Clean up temp file
        if (_tempHtmlFile != null && File.Exists(_tempHtmlFile))
        {
            try { File.Delete(_tempHtmlFile); } catch { }
        }
        base.OnClosed(e);
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
    
    private async void LoadMarkdownFromArgs()
    {
        var args = Environment.GetCommandLineArgs();
        
        if (args.Length > 1)
        {
            var filePath = args[1];
            
            // Try the file as-is first
            if (!File.Exists(filePath))
            {
                // If not found, try adding .md extension
                var mdFilePath = filePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase) 
                    ? filePath 
                    : filePath + ".md";
                    
                if (File.Exists(mdFilePath))
                {
                    filePath = mdFilePath;
                }
            }
            
            if (File.Exists(filePath))
            {
                try
                {
                    var content = await File.ReadAllTextAsync(filePath);
                    var html = Markdown.ToHtml(content, _pipeline);
                    await OpenInBrowser(html, Path.GetFileName(filePath));
                    Title = $"Markdown Viewer - {Path.GetFileName(filePath)}";
                }
                catch (Exception ex)
                {
                    var errorHtml = $"<h1>Error loading file</h1><pre>{ex.Message}</pre>";
                    await OpenInBrowser(errorHtml, "Error");
                    Title = "Markdown Viewer - Error";
                }
            }
            else
            {
                var notFoundHtml = $"<h1>File not found</h1><p>The file '<code>{filePath}</code>' could not be found.</p>";
                await OpenInBrowser(notFoundHtml, "File Not Found");
                Title = "Markdown Viewer - File Not Found";
            }
        }
        else
        {
            await LoadUsageFile();
        }
    }
    
    
    private async Task LoadUsageFile()
    {
        var usageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "usage.md");
        
        if (File.Exists(usageFilePath))
        {
            try
            {
                var content = await File.ReadAllTextAsync(usageFilePath);
                var html = Markdown.ToHtml(content, _pipeline);
                await OpenInBrowser(html, "Usage");
                Title = "Markdown Viewer - Usage";
            }
            catch (Exception ex)
            {
                var errorHtml = $"<h1>Error loading usage file</h1><pre>{ex.Message}</pre>";
                await OpenInBrowser(errorHtml, "Error");
                Title = "Markdown Viewer - Error";
            }
        }
        else
        {
            var defaultHtml = "<h1>Markdown Viewer</h1><p>Usage: <code>mdv &lt;file.md&gt;</code></p><p>Press <strong>ESC</strong> to quit.</p>";
            await OpenInBrowser(defaultHtml, "Usage");
            Title = "Markdown Viewer - Usage";
        }
    }
    
    private async Task OpenInBrowser(string htmlContent, string title)
    {
        var fullHtml = CreateFullHtmlWithMermaid(htmlContent, title);
        
        // Create temporary HTML file
        _tempHtmlFile = Path.Combine(Path.GetTempPath(), $"mdv_{Guid.NewGuid()}.html");
        await File.WriteAllTextAsync(_tempHtmlFile, fullHtml);
        
        // Open in default browser
        OpenUrl($"file://{_tempHtmlFile}");
        
        // Update status and close app after delay
        StatusText.Text = $"Opened {title} in browser. Press ESC to quit.";
        
        // Auto-close after 3 seconds
        await Task.Delay(3000);
        if (!IsVisible) return; // Don't close if already closed
        Close();
    }
    
    private void OpenUrl(string url)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error opening browser: {ex.Message}";
        }
    }
    
    private string CreateFullHtmlWithMermaid(string contentHtml, string title)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>{title} - mdv</title>
    <script src=""https://cdn.jsdelivr.net/npm/mermaid@10.6.1/dist/mermaid.min.js""></script>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            font-size: 16px;
            line-height: 1.6;
            color: #333;
            max-width: 900px;
            margin: 0 auto;
            padding: 20px;
            background-color: #fff;
        }}
        
        h1, h2, h3, h4, h5, h6 {{
            color: #2c3e50;
            margin-top: 1.5em;
            margin-bottom: 0.5em;
        }}
        
        h1 {{ border-bottom: 2px solid #eee; padding-bottom: 0.3em; }}
        h2 {{ border-bottom: 1px solid #eee; padding-bottom: 0.3em; }}
        
        code {{
            background-color: #f8f8f8;
            padding: 2px 6px;
            border-radius: 4px;
            font-family: 'SF Mono', Consolas, 'Liberation Mono', Menlo, monospace;
            font-size: 0.85em;
            border: 1px solid #e1e8ed;
        }}
        
        pre {{
            background-color: #f8f8f8;
            padding: 16px;
            border-radius: 6px;
            overflow-x: auto;
            border: 1px solid #e1e8ed;
            line-height: 1.45;
        }}
        
        pre code {{
            background: none;
            border: none;
            padding: 0;
            font-size: 0.875em;
        }}
        
        blockquote {{
            border-left: 4px solid #dfe2e5;
            padding-left: 16px;
            margin-left: 0;
            color: #6a737d;
            font-style: italic;
        }}
        
        table {{
            border-collapse: collapse;
            width: 100%;
            margin: 1em 0;
        }}
        
        th, td {{
            border: 1px solid #dfe2e5;
            padding: 8px 12px;
            text-align: left;
        }}
        
        th {{
            background-color: #f6f8fa;
            font-weight: 600;
        }}
        
        tr:nth-child(even) {{
            background-color: #f6f8fa;
        }}
        
        a {{
            color: #0366d6;
            text-decoration: none;
        }}
        
        a:hover {{
            text-decoration: underline;
        }}
        
        ul, ol {{
            padding-left: 2em;
        }}
        
        li {{
            margin: 0.25em 0;
        }}
        
        /* Mermaid diagram styling */
        .mermaid {{
            text-align: center;
            margin: 1em 0;
        }}
        
        /* Print styles */
        @media print {{
            body {{
                max-width: none;
                margin: 0;
                padding: 1cm;
            }}
        }}
        
        /* Dark mode support */
        @media (prefers-color-scheme: dark) {{
            body {{
                background-color: #1a1a1a;
                color: #e1e1e1;
            }}
            
            h1, h2, h3, h4, h5, h6 {{
                color: #ffffff;
            }}
            
            code, pre {{
                background-color: #2d2d2d;
                color: #e1e1e1;
                border-color: #404040;
            }}
            
            blockquote {{
                border-left-color: #404040;
                color: #b0b0b0;
            }}
            
            table th, table td {{
                border-color: #404040;
            }}
            
            table th {{
                background-color: #2d2d2d;
            }}
            
            table tr:nth-child(even) {{
                background-color: #252525;
            }}
        }}
    </style>
</head>
<body>
    {contentHtml}
    
    <script>
        // Initialize Mermaid
        mermaid.initialize({{
            startOnLoad: true,
            theme: window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'default',
            securityLevel: 'loose',
            fontFamily: 'inherit'
        }});
        
        // Convert code blocks with language 'mermaid' to actual Mermaid diagrams
        document.addEventListener('DOMContentLoaded', function() {{
            const mermaidBlocks = document.querySelectorAll('pre code.language-mermaid, pre code[class*=""language-mermaid""]');
            mermaidBlocks.forEach((block, index) => {{
                const code = block.textContent;
                const container = document.createElement('div');
                container.className = 'mermaid';
                container.id = 'mermaid-' + index;
                container.textContent = code;
                block.parentElement.parentElement.replaceChild(container, block.parentElement);
            }});
            
            // Re-render Mermaid diagrams
            if (typeof mermaid !== 'undefined') {{
                mermaid.init(undefined, '.mermaid');
            }}
        }});
        
        // Auto-focus for better keyboard navigation
        document.body.focus();
        
        // Handle keyboard shortcuts
        document.addEventListener('keydown', function(e) {{
            if (e.key === 'Escape') {{
                window.close();
            }}
        }});
    </script>
</body>
</html>";
    }
}