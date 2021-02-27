using Lockdown.Commands;
using System;
using Xunit;
using Lockdown.Tests.Utils;
using Shouldly;
using Moq;
using Lockdown.Build;

namespace Lockdown.Tests
{
    public class BuildCommandTests
    {
        [Fact]
        public void TestWriteToConsole()
        {
            // Setup
            var testConsole = new TestConsole();
            var mockSiteBuilder = new Mock<ISiteBuilder>();
            var inputPath = "./luis_rosales";
            var outputPath = "./ariel/likes";
            var buildCommand = new BuildCommand(testConsole, siteBuilder: mockSiteBuilder.Object);
            buildCommand.InputPath = inputPath;
            buildCommand.OutputPath = outputPath;

            // Act
            buildCommand.OnExecute();

            // Assert
            mockSiteBuilder.Verify(sb => sb.Build(inputPath, outputPath), Times.Once);
        }
    }
}
