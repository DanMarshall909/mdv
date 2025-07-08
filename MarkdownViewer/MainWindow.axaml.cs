using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Markdig;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace MarkdownViewer;

public partial class MainWindow : Window
{
    private readonly MarkdownPipeline _pipeline;
    private bool _useBrowserMode = false;
    private bool _isFullscreen = false;
    private WindowState _previousWindowState;
    private Border? _escapeOverlay;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
            
        LoadMarkdownFromArgs();
        KeyDown += OnKeyDown;
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (_isFullscreen)
            {
                ExitFullscreen();
            }
            else
            {
                Close();
            }
        }
        else if (e.Key == Key.B && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            ToggleDisplayMode();
        }
        else if (e.Key == Key.F11)
        {
            ToggleFullscreen();
        }
        else if (e.Key == Key.C && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                CopyAsRichText();
            }
            else if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                CopyAsHtml();
            }
            else
            {
                CopyAsPlainText();
            }
        }
        else if (e.Key == Key.M && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            CopyAsMarkdown();
        }
    }
    
    private async void ToggleDisplayMode()
    {
        _useBrowserMode = !_useBrowserMode;
        
        // Show a brief notification
        var originalTitle = Title;
        Title = _useBrowserMode ? "Mode: Browser" : "Mode: In-App";
        
        // Restore title after 1 second
        await Task.Delay(1000);
        Title = originalTitle;
        
        // Reload current content with new mode
        await ReloadCurrentContent();
    }
    
    private async Task ReloadCurrentContent()
    {
        var args = Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            var filePath = args[1];
            if (File.Exists(filePath))
            {
                var content = await File.ReadAllTextAsync(filePath);
                if (_useBrowserMode)
                {
                    await RenderMarkdownInBrowser(content);
                }
                else
                {
                    await RenderMarkdownInApp(content);
                }
            }
        }
    }
    
    private void ToggleFullscreen()
    {
        if (_isFullscreen)
        {
            ExitFullscreen();
        }
        else
        {
            EnterFullscreen();
        }
    }
    
    private void EnterFullscreen()
    {
        _previousWindowState = WindowState;
        _isFullscreen = true;
        
        WindowState = WindowState.FullScreen;
        SystemDecorations = SystemDecorations.None;
        
        ShowEscapeOverlay();
    }
    
    private void ExitFullscreen()
    {
        _isFullscreen = false;
        
        WindowState = _previousWindowState;
        SystemDecorations = SystemDecorations.Full;
        
        HideEscapeOverlay();
    }
    
    private void ShowEscapeOverlay()
    {
        if (_escapeOverlay != null) return;
        
        _escapeOverlay = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Margin = new Avalonia.Thickness(0, 20, 0, 0),
            CornerRadius = new Avalonia.CornerRadius(15),
            Padding = new Avalonia.Thickness(20, 10, 20, 10),
            Child = new TextBlock
            {
                Text = "Press ESC to exit fullscreen",
                Foreground = Brushes.White,
                FontSize = 14
            }
        };
        
        // Add overlay to the main grid
        if (Content is ScrollViewer scrollViewer && scrollViewer.Content is Border border)
        {
            var grid = new Grid();
            grid.Children.Add(border);
            grid.Children.Add(_escapeOverlay);
            scrollViewer.Content = grid;
        }
        
        // Auto-hide after 3 seconds
        var timer = new System.Timers.Timer(3000);
        timer.Elapsed += (s, e) =>
        {
            timer.Stop();
            Avalonia.Threading.Dispatcher.UIThread.Post(HideEscapeOverlay);
        };
        timer.Start();
    }
    
    private void HideEscapeOverlay()
    {
        if (_escapeOverlay == null) return;
        
        // Remove overlay from the grid
        if (Content is ScrollViewer scrollViewer && scrollViewer.Content is Grid grid)
        {
            grid.Children.Remove(_escapeOverlay);
            if (grid.Children.Count == 1)
            {
                scrollViewer.Content = grid.Children[0];
            }
        }
        
        _escapeOverlay = null;
    }
    
    private void CopyAsPlainText()
    {
        var plainText = ExtractPlainText();
        CopyToClipboard(plainText);
        ShowCopyNotification("Copied as plain text");
    }
    
    private void CopyAsRichText()
    {
        // For now, use plain text - rich text requires more complex implementation
        var plainText = ExtractPlainText();
        CopyToClipboard(plainText);
        ShowCopyNotification("Copied as rich text");
    }
    
    private void CopyAsHtml()
    {
        var html = ExtractHtml();
        CopyToClipboard(html);
        ShowCopyNotification("Copied as HTML");
    }
    
    private void CopyAsMarkdown()
    {
        var markdown = ExtractMarkdown();
        CopyToClipboard(markdown);
        ShowCopyNotification("Copied as markdown");
    }
    
    private string ExtractPlainText()
    {
        var text = new StringBuilder();
        ExtractTextFromChildren(ContentPanel.Children, text);
        return text.ToString();
    }
    
    private void ExtractTextFromChildren(Avalonia.Controls.Controls children, StringBuilder text)
    {
        foreach (var child in children)
        {
            if (child is TextBlock textBlock)
            {
                text.AppendLine(textBlock.Text);
            }
            else if (child is SelectableTextBlock selectableTextBlock)
            {
                text.AppendLine(selectableTextBlock.Text);
            }
            else if (child is Panel panel)
            {
                ExtractTextFromChildren(panel.Children, text);
            }
            else if (child is Border border && border.Child != null)
            {
                if (border.Child is Panel borderPanel)
                {
                    ExtractTextFromChildren(borderPanel.Children, text);
                }
                else if (border.Child is TextBlock borderText)
                {
                    text.AppendLine(borderText.Text);
                }
            }
        }
    }
    
    private string ExtractHtml()
    {
        // Return the current markdown converted to HTML
        var args = Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            var filePath = args[1];
            if (File.Exists(filePath))
            {
                var content = File.ReadAllText(filePath);
                return Markdown.ToHtml(content, _pipeline);
            }
        }
        return "";
    }
    
    private string ExtractMarkdown()
    {
        // Return the original markdown content
        var args = Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            var filePath = args[1];
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
        }
        return "";
    }
    
    private async void CopyToClipboard(string text)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(text);
            }
        }
        catch (Exception)
        {
            // Silently ignore clipboard errors
        }
    }
    
    private async void ShowCopyNotification(string message)
    {
        var originalTitle = Title;
        Title = message;
        await Task.Delay(1000);
        Title = originalTitle;
    }
    
    private async void LoadMarkdownFromArgs()
    {
        var args = Environment.GetCommandLineArgs();
        
        // Parse command line arguments
        var parsedArgs = ArgumentParser.Parse(args);
        _useBrowserMode = parsedArgs.BrowserMode;
        
        if (parsedArgs.FullscreenMode)
        {
            // Start in fullscreen after loading
            Loaded += (s, e) => ToggleFullscreen();
        }
        
        var filePath = parsedArgs.FilePath;
        
        if (!string.IsNullOrEmpty(filePath))
        {
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
                    await RenderMarkdown(content);
                    Title = $"Markdown Viewer - {Path.GetFileName(filePath)} ({(_useBrowserMode ? "Browser" : "In-App")})";
                }
                catch (Exception ex)
                {
                    var errorMarkdown = $"# Error loading file\n\n```\n{ex.Message}\n```";
                    await RenderMarkdown(errorMarkdown);
                    Title = "Markdown Viewer - Error";
                }
            }
            else
            {
                var notFoundMarkdown = $"# File not found\n\nThe file `{filePath}` could not be found.";
                await RenderMarkdown(notFoundMarkdown);
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
                await RenderMarkdown(content);
                Title = "Markdown Viewer - Usage";
            }
            catch (Exception ex)
            {
                var errorMarkdown = $"# Error loading usage file\n\n```\n{ex.Message}\n```";
                await RenderMarkdown(errorMarkdown);
                Title = "Markdown Viewer - Error";
            }
        }
        else
        {
            var defaultMarkdown = new ArgumentParser().GetUsageText();
            await RenderMarkdown(defaultMarkdown);
            Title = "Markdown Viewer - Usage";
        }
    }
    
    private async Task RenderMarkdown(string markdown)
    {
        if (_useBrowserMode)
        {
            await RenderMarkdownInBrowser(markdown);
        }
        else
        {
            await RenderMarkdownInApp(markdown);
        }
    }
    
    private async Task RenderMarkdownInApp(string markdown)
    {
        // Convert markdown to HTML using Markdig
        var html = Markdown.ToHtml(markdown, _pipeline);
        
        // Render HTML to Avalonia controls
        ContentPanel.Children.Clear();
        
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        
        foreach (var node in doc.DocumentNode.ChildNodes)
        {
            var control = CreateControlFromHtmlNode(node);
            if (control != null)
                ContentPanel.Children.Add(control);
        }
        
        await Task.CompletedTask;
    }
    
    private async Task RenderMarkdownInBrowser(string markdown)
    {
        var htmlContent = GetFullHtmlTemplate(markdown);
        var tempFile = Path.GetTempFileName() + ".html";
        await File.WriteAllTextAsync(tempFile, htmlContent);
        
        // Open in default browser
        try
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                Process.Start("xdg-open", tempFile);
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                Process.Start("open", tempFile);
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            // If browser opening fails, show content in the Avalonia window
            ContentPanel.Children.Clear();
            ContentPanel.Children.Add(new TextBlock
            {
                Text = $"Could not open browser: {ex.Message}\n\nFile saved to: {tempFile}",
                FontSize = 16,
                TextAlignment = TextAlignment.Center,
                Margin = new Avalonia.Thickness(0, 100, 0, 0)
            });
            return;
        }
        
        // For the Avalonia window, show a simple message
        ContentPanel.Children.Clear();
        ContentPanel.Children.Add(new StackPanel
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Children =
            {
                new TextBlock
                {
                    Text = "🌐 Markdown opened in browser",
                    FontSize = 18,
                    FontWeight = FontWeight.Bold,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Avalonia.Thickness(0, 0, 0, 10)
                },
                new TextBlock
                {
                    Text = "Press Ctrl+B to switch to in-app mode\nPress ESC to close",
                    FontSize = 14,
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(106, 115, 125))
                }
            }
        });
    }
    
    private Control? CreateControlFromHtmlNode(HtmlNode node)
    {
        switch (node.Name.ToLower())
        {
            case "h1":
                return new TextBlock
                {
                    Text = node.InnerText,
                    FontSize = 28,
                    FontWeight = FontWeight.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                    Margin = new Avalonia.Thickness(0, 24, 0, 12)
                };
                
            case "h2":
                return new TextBlock
                {
                    Text = node.InnerText,
                    FontSize = 24,
                    FontWeight = FontWeight.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                    Margin = new Avalonia.Thickness(0, 20, 0, 10)
                };
                
            case "h3":
                return new TextBlock
                {
                    Text = node.InnerText,
                    FontSize = 20,
                    FontWeight = FontWeight.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                    Margin = new Avalonia.Thickness(0, 16, 0, 8)
                };
                
            case "p":
                return CreateRichTextBlock(node);
                
            case "pre":
                var codeNode = node.SelectSingleNode(".//code");
                var codeText = codeNode?.InnerText ?? node.InnerText;
                var codeClass = codeNode?.GetAttributeValue("class", "") ?? "";
                
                // Check for Mermaid diagrams
                if (codeClass.Contains("language-mermaid") || codeText.TrimStart().StartsWith("graph") || 
                    codeText.TrimStart().StartsWith("flowchart") || codeText.TrimStart().StartsWith("sequenceDiagram"))
                {
                    return new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(240, 248, 255)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(79, 172, 254)),
                        BorderThickness = new Avalonia.Thickness(2),
                        CornerRadius = new Avalonia.CornerRadius(8),
                        Padding = new Avalonia.Thickness(20),
                        Margin = new Avalonia.Thickness(0, 12, 0, 12),
                        Child = new StackPanel
                        {
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = "📊 Mermaid Diagram",
                                    FontWeight = FontWeight.Bold,
                                    FontSize = 16,
                                    Foreground = new SolidColorBrush(Color.FromRgb(79, 172, 254)),
                                    Margin = new Avalonia.Thickness(0, 0, 0, 10)
                                },
                                new StackPanel
                                {
                                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                                    Margin = new Avalonia.Thickness(0, 0, 0, 10),
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Text = "Diagram detected.",
                                            FontStyle = FontStyle.Italic,
                                            FontSize = 12,
                                            Foreground = new SolidColorBrush(Color.FromRgb(106, 115, 125)),
                                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                                        },
                                        CreateMermaidButton(codeText)
                                    }
                                },
                                new SelectableTextBlock
                                {
                                    Text = codeText,
                                    FontFamily = new FontFamily("Consolas,Monaco,monospace"),
                                    FontSize = 12,
                                    Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                                }
                            }
                        }
                    };
                }
                
                return new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(248, 248, 248)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(225, 232, 237)),
                    BorderThickness = new Avalonia.Thickness(1),
                    CornerRadius = new Avalonia.CornerRadius(6),
                    Padding = new Avalonia.Thickness(16),
                    Margin = new Avalonia.Thickness(0, 12, 0, 12),
                    Child = new SelectableTextBlock
                    {
                        Text = codeText,
                        FontFamily = new FontFamily("Consolas,Monaco,monospace"),
                        FontSize = 14,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    }
                };
                
            case "blockquote":
                return new Border
                {
                    BorderBrush = new SolidColorBrush(Color.FromRgb(223, 226, 229)),
                    BorderThickness = new Avalonia.Thickness(4, 0, 0, 0),
                    Padding = new Avalonia.Thickness(16, 12, 12, 12),
                    Margin = new Avalonia.Thickness(0, 12, 0, 12),
                    Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                    Child = new TextBlock
                    {
                        Text = node.InnerText,
                        FontStyle = FontStyle.Italic,
                        FontSize = 16,
                        Foreground = new SolidColorBrush(Color.FromRgb(106, 115, 125)),
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    }
                };
                
            case "ul":
                return CreateListControl(node, false);
                
            case "ol":
                return CreateListControl(node, true);
                
            case "table":
                return CreateTableControl(node);
                
            case "hr":
                return new Border
                {
                    Height = 1,
                    Background = new SolidColorBrush(Color.FromRgb(223, 226, 229)),
                    Margin = new Avalonia.Thickness(0, 20, 0, 20)
                };
                
            default:
                if (!string.IsNullOrWhiteSpace(node.InnerText))
                {
                    return new TextBlock
                    {
                        Text = node.InnerText,
                        FontSize = 16,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        Margin = new Avalonia.Thickness(0, 6, 0, 6)
                    };
                }
                return null;
        }
    }
    
    private Control CreateRichTextBlock(HtmlNode node)
    {
        var panel = new WrapPanel();
        
        foreach (var child in node.ChildNodes)
        {
            Control? childControl = null;
            
            if (child.NodeType == HtmlNodeType.Text && !string.IsNullOrWhiteSpace(child.InnerText))
            {
                childControl = new TextBlock
                {
                    Text = child.InnerText,
                    FontSize = 16,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(0, 6, 0, 12)
                };
            }
            else if (child.Name == "code")
            {
                childControl = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(248, 248, 248)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(225, 232, 237)),
                    BorderThickness = new Avalonia.Thickness(1),
                    CornerRadius = new Avalonia.CornerRadius(4),
                    Padding = new Avalonia.Thickness(6, 2, 6, 2),
                    Margin = new Avalonia.Thickness(2, 0, 2, 0),
                    Child = new TextBlock
                    {
                        Text = child.InnerText,
                        FontFamily = new FontFamily("Consolas,Monaco,monospace"),
                        FontSize = 14
                    }
                };
            }
            else if (child.Name == "strong")
            {
                childControl = new TextBlock
                {
                    Text = child.InnerText,
                    FontWeight = FontWeight.Bold,
                    FontSize = 16,
                    Margin = new Avalonia.Thickness(0, 6, 0, 12)
                };
            }
            else if (child.Name == "em")
            {
                childControl = new TextBlock
                {
                    Text = child.InnerText,
                    FontStyle = FontStyle.Italic,
                    FontSize = 16,
                    Margin = new Avalonia.Thickness(0, 6, 0, 12)
                };
            }
            else if (child.Name == "a")
            {
                childControl = new TextBlock
                {
                    Text = child.InnerText,
                    FontSize = 16,
                    Foreground = new SolidColorBrush(Color.FromRgb(3, 102, 214)),
                    TextDecorations = TextDecorations.Underline,
                    Margin = new Avalonia.Thickness(0, 6, 0, 12)
                };
            }
            
            if (childControl != null)
                panel.Children.Add(childControl);
        }
        
        if (panel.Children.Count == 0)
        {
            return new TextBlock
            {
                Text = node.InnerText,
                FontSize = 16,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(0, 6, 0, 12)
            };
        }
        
        return panel;
    }
    
    private Control CreateListControl(HtmlNode node, bool isOrdered)
    {
        var listPanel = new StackPanel { Margin = new Avalonia.Thickness(0, 12, 0, 12) };
        var counter = 1;
        
        foreach (var li in node.SelectNodes(".//li") ?? new HtmlNodeCollection(node))
        {
            var bullet = isOrdered ? $"{counter++}. " : "• ";
            listPanel.Children.Add(new TextBlock
            {
                Text = bullet + li.InnerText,
                FontSize = 16,
                Margin = new Avalonia.Thickness(24, 3, 0, 3),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            });
        }
        
        return listPanel;
    }
    
    private Control CreateTableControl(HtmlNode node)
    {
        var grid = new Grid
        {
            Margin = new Avalonia.Thickness(0, 12, 0, 12)
        };
        
        var rows = node.SelectNodes(".//tr");
        if (rows == null || !rows.Any()) return grid;
        
        // Define columns based on first row
        var firstRow = rows[0];
        var cellCount = firstRow.SelectNodes(".//th | .//td")?.Count ?? 0;
        
        for (int i = 0; i < cellCount; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }
        
        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var cells = rows[rowIndex].SelectNodes(".//th | .//td");
            
            if (cells != null)
            {
                for (int colIndex = 0; colIndex < Math.Min(cells.Count, cellCount); colIndex++)
                {
                    var isHeader = cells[colIndex].Name == "th";
                    var border = new Border
                    {
                        BorderBrush = new SolidColorBrush(Color.FromRgb(223, 226, 229)),
                        BorderThickness = new Avalonia.Thickness(1),
                        Padding = new Avalonia.Thickness(12, 8, 12, 8),
                        Background = isHeader ? new SolidColorBrush(Color.FromRgb(246, 248, 250)) : Brushes.Transparent,
                        Child = new TextBlock
                        {
                            Text = cells[colIndex].InnerText,
                            FontWeight = isHeader ? FontWeight.Bold : FontWeight.Normal,
                            FontSize = 14,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        }
                    };
                    
                    Grid.SetRow(border, rowIndex);
                    Grid.SetColumn(border, colIndex);
                    grid.Children.Add(border);
                }
            }
        }
        
        return grid;
    }
    
    private Button CreateMermaidButton(string mermaidCode)
    {
        var button = new Button
        {
            Content = "🌐 View in Browser",
            FontSize = 11,
            Padding = new Avalonia.Thickness(8, 4, 8, 4),
            Margin = new Avalonia.Thickness(10, 0, 0, 0),
            Background = new SolidColorBrush(Color.FromRgb(79, 172, 254)),
            Foreground = Brushes.White,
            CornerRadius = new Avalonia.CornerRadius(4)
        };
        
        button.Click += async (sender, e) =>
        {
            await OpenMermaidInBrowser(mermaidCode);
        };
        
        return button;
    }
    
    private async Task OpenMermaidInBrowser(string mermaidCode)
    {
        var htmlContent = GetMermaidHtmlTemplate(mermaidCode);
        var tempFile = Path.GetTempFileName() + ".html";
        await File.WriteAllTextAsync(tempFile, htmlContent);
        
        try
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                Process.Start("xdg-open", tempFile);
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                Process.Start("open", tempFile);
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception)
        {
            // Silently ignore browser opening errors
        }
    }
    
    private string GetFullHtmlTemplate(string markdown)
    {
        var markdownEscaped = markdown.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
        
        return @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Markdown Viewer</title>
    <script src=""https://cdn.jsdelivr.net/npm/marked@11.1.1/marked.min.js""></script>
    <script src=""https://cdn.jsdelivr.net/npm/dompurify@3.0.6/dist/purify.min.js""></script>
    <script src=""https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-core.min.js""></script>
    <script src=""https://cdn.jsdelivr.net/npm/prismjs@1.29.0/plugins/autoloader/prism-autoloader.min.js""></script>
    <link href=""https://cdn.jsdelivr.net/npm/prismjs@1.29.0/themes/prism.min.css"" rel=""stylesheet"">
    <script src=""https://cdn.jsdelivr.net/npm/mermaid@10.6.1/dist/mermaid.min.js""></script>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 1200px;
            margin: 0 auto;
            padding: 40px 20px;
            background: #fff;
        }
        h1, h2, h3, h4, h5, h6 {
            color: #2c3e50;
            margin-top: 30px;
            margin-bottom: 15px;
        }
        h1 { font-size: 2em; }
        h2 { font-size: 1.6em; }
        h3 { font-size: 1.4em; }
        code {
            background: #f8f8f8;
            padding: 2px 6px;
            border-radius: 4px;
            font-family: 'Consolas', 'Monaco', monospace;
        }
        pre {
            background: #f8f8f8;
            padding: 16px;
            border-radius: 6px;
            overflow-x: auto;
            border: 1px solid #e1e8ed;
        }
        pre code {
            background: transparent;
            padding: 0;
        }
        blockquote {
            border-left: 4px solid #dfe2e5;
            padding-left: 16px;
            margin: 12px 0;
            color: #6a737d;
            font-style: italic;
        }
        table {
            border-collapse: collapse;
            width: 100%;
            margin: 12px 0;
        }
        th, td {
            border: 1px solid #dfe2e5;
            padding: 12px 8px;
            text-align: left;
        }
        th {
            background: #f6f8fa;
            font-weight: bold;
        }
        ul, ol {
            padding-left: 24px;
        }
        li {
            margin: 3px 0;
        }
        a {
            color: #0366d6;
            text-decoration: none;
        }
        a:hover {
            text-decoration: underline;
        }
        .mermaid {
            background: #f0f8ff;
            border: 2px solid #4facfe;
            border-radius: 8px;
            padding: 20px;
            margin: 12px 0;
        }
    </style>
</head>
<body>
    <div id=""content""></div>
    <script>
        // Configure marked
        marked.setOptions({
            highlight: function(code, lang) {
                if (lang === 'mermaid') {
                    return '<div class=""mermaid"">' + code + '</div>';
                }
                return code;
            },
            breaks: true,
            gfm: true
        });
        
        // Initialize mermaid
        mermaid.initialize({ startOnLoad: true });
        
        // Render markdown
        const markdown = """ + markdownEscaped + @""";
        const html = marked.parse(markdown);
        const cleanHtml = DOMPurify.sanitize(html);
        document.getElementById('content').innerHTML = cleanHtml;
        
        // Re-run mermaid after content is loaded
        mermaid.run();
        
        // Handle ESC key
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape') {
                window.close();
            }
        });
    </script>
</body>
</html>";
    }
    
    private string GetMermaidHtmlTemplate(string mermaidCode)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Mermaid Diagram</title>
    <script src=""https://cdn.jsdelivr.net/npm/mermaid@10.6.1/dist/mermaid.min.js""></script>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            margin: 20px;
            background: #fff;
        }}
        .mermaid {{
            text-align: center;
        }}
        h1 {{
            color: #333;
            text-align: center;
        }}
    </style>
</head>
<body>
    <h1>📊 Mermaid Diagram</h1>
    <div class=""mermaid"">
{mermaidCode}
    </div>
    <script>
        mermaid.initialize({{ startOnLoad: true, theme: 'default' }});
    </script>
</body>
</html>";
    }
}