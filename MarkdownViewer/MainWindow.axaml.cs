using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Markdig;
using System;
using System.IO;
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
                    var html = Markdown.ToHtml(content, _pipeline);
                    RenderHtmlToControls(html);
                    Title = $"Markdown Viewer - {Path.GetFileName(filePath)}";
                }
                catch (Exception ex)
                {
                    var errorHtml = $"<h1>Error loading file</h1><pre>{ex.Message}</pre>";
                    RenderHtmlToControls(errorHtml);
                    Title = "Markdown Viewer - Error";
                }
            }
            else
            {
                var notFoundHtml = $"<h1>File not found</h1><p>The file '<code>{filePath}</code>' could not be found.</p>";
                RenderHtmlToControls(notFoundHtml);
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
                RenderHtmlToControls(html);
                Title = "Markdown Viewer - Usage";
            }
            catch (Exception ex)
            {
                var errorHtml = $"<h1>Error loading usage file</h1><pre>{ex.Message}</pre>";
                RenderHtmlToControls(errorHtml);
                Title = "Markdown Viewer - Error";
            }
        }
        else
        {
            var defaultHtml = "<h1>Markdown Viewer</h1><p>Usage: <code>MarkdownViewer &lt;file.md&gt;</code></p><p>Press <strong>ESC</strong> to quit.</p>";
            RenderHtmlToControls(defaultHtml);
            Title = "Markdown Viewer - Usage";
        }
    }
    
    private void RenderHtmlToControls(string html)
    {
        ContentPanel.Children.Clear();
        
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        
        foreach (var node in doc.DocumentNode.ChildNodes)
        {
            var control = CreateControlFromHtmlNode(node);
            if (control != null)
                ContentPanel.Children.Add(control);
        }
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
                    Margin = new Avalonia.Thickness(0, 20, 0, 10)
                };
                
            case "h2":
                return new TextBlock
                {
                    Text = node.InnerText,
                    FontSize = 24,
                    FontWeight = FontWeight.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                    Margin = new Avalonia.Thickness(0, 16, 0, 8)
                };
                
            case "h3":
                return new TextBlock
                {
                    Text = node.InnerText,
                    FontSize = 20,
                    FontWeight = FontWeight.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                    Margin = new Avalonia.Thickness(0, 12, 0, 6)
                };
                
            case "p":
                return CreateParagraphControl(node);
                
            case "pre":
                var codeNode = node.SelectSingleNode(".//code");
                var codeText = codeNode?.InnerText ?? node.InnerText;
                
                // Check for Mermaid diagrams
                if (codeNode?.GetAttributeValue("class", "").Contains("language-mermaid") == true)
                {
                    return new TextBlock
                    {
                        Text = $"[Mermaid Diagram]\n{codeText}",
                        FontFamily = new FontFamily("Consolas,Monaco,monospace"),
                        Background = new SolidColorBrush(Color.FromRgb(240, 248, 255)),
                        Padding = new Avalonia.Thickness(16),
                        Margin = new Avalonia.Thickness(0, 8, 0, 8),
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    };
                }
                
                return new TextBlock
                {
                    Text = codeText,
                    FontFamily = new FontFamily("Consolas,Monaco,monospace"),
                    FontSize = 14,
                    Background = new SolidColorBrush(Color.FromRgb(248, 248, 248)),
                    Padding = new Avalonia.Thickness(16),
                    Margin = new Avalonia.Thickness(0, 8, 0, 8),
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                };
                
            case "blockquote":
                return new Border
                {
                    BorderBrush = new SolidColorBrush(Color.FromRgb(223, 226, 229)),
                    BorderThickness = new Avalonia.Thickness(4, 0, 0, 0),
                    Padding = new Avalonia.Thickness(16, 8, 8, 8),
                    Margin = new Avalonia.Thickness(0, 8, 0, 8),
                    Child = new TextBlock
                    {
                        Text = node.InnerText,
                        FontStyle = FontStyle.Italic,
                        Foreground = new SolidColorBrush(Color.FromRgb(106, 115, 125)),
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    }
                };
                
            case "ul":
                return CreateListControl(node, false);
                
            case "ol":
                return CreateListControl(node, true);
                
            default:
                if (!string.IsNullOrWhiteSpace(node.InnerText))
                {
                    return new TextBlock
                    {
                        Text = node.InnerText,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        Margin = new Avalonia.Thickness(0, 4, 0, 4)
                    };
                }
                return null;
        }
    }
    
    private Control CreateParagraphControl(HtmlNode node)
    {
        var panel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
        
        foreach (var child in node.ChildNodes)
        {
            if (child.NodeType == HtmlNodeType.Text)
            {
                if (!string.IsNullOrWhiteSpace(child.InnerText))
                {
                    panel.Children.Add(new TextBlock
                    {
                        Text = child.InnerText,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        Margin = new Avalonia.Thickness(0, 4, 0, 8)
                    });
                }
            }
            else if (child.Name == "code")
            {
                panel.Children.Add(new TextBlock
                {
                    Text = child.InnerText,
                    FontFamily = new FontFamily("Consolas,Monaco,monospace"),
                    Background = new SolidColorBrush(Color.FromRgb(248, 248, 248)),
                    Padding = new Avalonia.Thickness(4, 2, 4, 2),
                    Margin = new Avalonia.Thickness(2, 0, 2, 0)
                });
            }
            else if (child.Name == "strong")
            {
                panel.Children.Add(new TextBlock
                {
                    Text = child.InnerText,
                    FontWeight = FontWeight.Bold,
                    Margin = new Avalonia.Thickness(0, 4, 0, 8)
                });
            }
            else if (child.Name == "em")
            {
                panel.Children.Add(new TextBlock
                {
                    Text = child.InnerText,
                    FontStyle = FontStyle.Italic,
                    Margin = new Avalonia.Thickness(0, 4, 0, 8)
                });
            }
        }
        
        if (panel.Children.Count == 0)
        {
            return new TextBlock
            {
                Text = node.InnerText,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(0, 4, 0, 8)
            };
        }
        
        return panel;
    }
    
    private Control CreateListControl(HtmlNode node, bool isOrdered)
    {
        var listPanel = new StackPanel { Margin = new Avalonia.Thickness(0, 8, 0, 8) };
        var counter = 1;
        
        foreach (var li in node.SelectNodes(".//li") ?? new HtmlNodeCollection(node))
        {
            var bullet = isOrdered ? $"{counter++}. " : "• ";
            listPanel.Children.Add(new TextBlock
            {
                Text = bullet + li.InnerText,
                Margin = new Avalonia.Thickness(20, 2, 0, 2),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            });
        }
        
        return listPanel;
    }
}