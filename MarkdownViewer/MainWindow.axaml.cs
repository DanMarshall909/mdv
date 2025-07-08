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
                    await RenderMarkdownInApp(content);
                    Title = $"Markdown Viewer - {Path.GetFileName(filePath)}";
                }
                catch (Exception ex)
                {
                    var errorMarkdown = $"# Error loading file\n\n```\n{ex.Message}\n```";
                    await RenderMarkdownInApp(errorMarkdown);
                    Title = "Markdown Viewer - Error";
                }
            }
            else
            {
                var notFoundMarkdown = $"# File not found\n\nThe file `{filePath}` could not be found.";
                await RenderMarkdownInApp(notFoundMarkdown);
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
                await RenderMarkdownInApp(content);
                Title = "Markdown Viewer - Usage";
            }
            catch (Exception ex)
            {
                var errorMarkdown = $"# Error loading usage file\n\n```\n{ex.Message}\n```";
                await RenderMarkdownInApp(errorMarkdown);
                Title = "Markdown Viewer - Error";
            }
        }
        else
        {
            var defaultMarkdown = "# Markdown Viewer\n\nUsage: `mdv <file.md>`\n\nPress **ESC** to quit.";
            await RenderMarkdownInApp(defaultMarkdown);
            Title = "Markdown Viewer - Usage";
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