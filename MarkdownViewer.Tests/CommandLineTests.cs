using Xunit;
using MarkdownViewer;

namespace MarkdownViewer.Tests
{
    public class CommandLineTests
    {
        [Fact]
        public void ParseArguments_BrowserFlag_SetsBrowserMode()
        {
            // Arrange
            var args = new[] { "mdv.exe", "test.md", "--browser" };
            
            // Act
            var result = ArgumentParser.Parse(args);
            
            // Assert
            Assert.True(result.BrowserMode);
            Assert.Equal("test.md", result.FilePath);
        }
        
        [Fact]
        public void ParseArguments_ShortBrowserFlag_SetsBrowserMode()
        {
            // Arrange
            var args = new[] { "mdv.exe", "test.md", "-b" };
            
            // Act
            var result = ArgumentParser.Parse(args);
            
            // Assert
            Assert.True(result.BrowserMode);
            Assert.Equal("test.md", result.FilePath);
        }
        
        [Fact]
        public void ParseArguments_FullscreenFlag_SetsFullscreenMode()
        {
            // Arrange
            var args = new[] { "mdv.exe", "test.md", "--fullscreen" };
            
            // Act
            var result = ArgumentParser.Parse(args);
            
            // Assert
            Assert.True(result.FullscreenMode);
            Assert.Equal("test.md", result.FilePath);
        }
        
        [Fact]
        public void ParseArguments_ShortFullscreenFlag_SetsFullscreenMode()
        {
            // Arrange
            var args = new[] { "mdv.exe", "test.md", "-f" };
            
            // Act
            var result = ArgumentParser.Parse(args);
            
            // Assert
            Assert.True(result.FullscreenMode);
            Assert.Equal("test.md", result.FilePath);
        }
        
        [Fact]
        public void ParseArguments_AppFlag_SetsBrowserModeToFalse()
        {
            // Arrange
            var args = new[] { "mdv.exe", "test.md", "--app" };
            
            // Act
            var result = ArgumentParser.Parse(args);
            
            // Assert
            Assert.False(result.BrowserMode);
            Assert.Equal("test.md", result.FilePath);
        }
        
        [Fact]
        public void ParseArguments_MultipleFlags_SetsAllCorrectly()
        {
            // Arrange
            var args = new[] { "mdv.exe", "test.md", "--browser", "--fullscreen" };
            
            // Act
            var result = ArgumentParser.Parse(args);
            
            // Assert
            Assert.True(result.BrowserMode);
            Assert.True(result.FullscreenMode);
            Assert.Equal("test.md", result.FilePath);
        }
        
        [Fact]
        public void ParseArguments_NoFile_ReturnsNullFilePath()
        {
            // Arrange
            var args = new[] { "mdv.exe", "--browser" };
            
            // Act
            var result = ArgumentParser.Parse(args);
            
            // Assert
            Assert.True(result.BrowserMode);
            Assert.Null(result.FilePath);
        }
        
        [Fact]
        public void ParseArguments_UnknownFlag_IsIgnored()
        {
            // Arrange
            var args = new[] { "mdv.exe", "test.md", "--unknown-flag" };
            
            // Act
            var result = ArgumentParser.Parse(args);
            
            // Assert
            Assert.False(result.BrowserMode);
            Assert.False(result.FullscreenMode);
            Assert.Equal("test.md", result.FilePath);
        }
        
        [Theory]
        [InlineData("--browser", "-b")]
        [InlineData("--app", "-a")]
        [InlineData("--fullscreen", "-f")]
        public void ParseArguments_ShortAndLongFlags_AreEquivalent(string longFlag, string shortFlag)
        {
            // Arrange
            var argsLong = new[] { "mdv.exe", "test.md", longFlag };
            var argsShort = new[] { "mdv.exe", "test.md", shortFlag };
            
            // Act
            var resultLong = ArgumentParser.Parse(argsLong);
            var resultShort = ArgumentParser.Parse(argsShort);
            
            // Assert
            Assert.Equal(resultLong.BrowserMode, resultShort.BrowserMode);
            Assert.Equal(resultLong.FullscreenMode, resultShort.FullscreenMode);
            Assert.Equal(resultLong.FilePath, resultShort.FilePath);
        }
        
        [Fact]
        public void GetUsageText_ReturnsNonEmptyString()
        {
            // Arrange
            var parser = new ArgumentParser();
            
            // Act
            var usage = parser.GetUsageText();
            
            // Assert
            Assert.NotNull(usage);
            Assert.NotEmpty(usage);
            Assert.Contains("Usage", usage);
            Assert.Contains("Options", usage);
            Assert.Contains("Examples", usage);
        }
    }
}