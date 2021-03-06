namespace Lockdown.Tests.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using Lockdown.Build;
    using Shouldly;
    using Xunit;

    public class TestHelperFileSystem
    {
        private const string InputPath = "./input";

        private readonly IFileSystem fakeFileSystem;

        private readonly HelperFileSystem helperFileSystem;

        public TestHelperFileSystem()
        {
            // Setup: Prepare our test by copying our `demo` folder into our "fake" file system.
            var workspace = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../../"));
            var templatePath = Path.Combine(workspace, "Lockdown", "BlankTemplate", "templates");
            var dictionary = new Dictionary<string, MockFileData>();
            foreach (var path in Directory.EnumerateFiles(templatePath))
            {
                var fakePath = path.Replace(templatePath, Path.Combine(InputPath, "templates"));
                dictionary.Add(fakePath, new MockFileData(File.ReadAllBytes(path)));
            }

            this.fakeFileSystem = new MockFileSystem(dictionary);

            this.helperFileSystem = new HelperFileSystem(this.fakeFileSystem, InputPath);
        }

        [Theory]
        [InlineData("'post'")]
        [InlineData("'post.liquid'")]
        [InlineData("post")]
        [InlineData("post.liquid")]
        public void TestFindFileFindsFile(string templateName)
        {
            var expected = "./input/templates/post.liquid";

            var actual = this.helperFileSystem.FindFile(templateName);

            actual.ShouldBe(expected);
        }

        [Theory]
        [InlineData("'new-file'")]
        [InlineData("'new-file.html'")]
        public void TestDoesNotFindFile(string templateName)
        {
            var actual = this.helperFileSystem.FindFile(templateName);

            actual.ShouldBeNull();
        }

        [Theory]
        [InlineData("'new-file'")]
        [InlineData("'new-file.html'")]
        [InlineData("new-file")]
        [InlineData("new-file.html")]
        public void TestFindFileFindsFileOtherExtension(string templateName)
        {
            var expected = "./input/templates/new-file.html";
            this.fakeFileSystem.File.WriteAllText(expected, "lol");

            var actual = this.helperFileSystem.FindFile(templateName);

            actual.ShouldBe(expected);
        }
    }
}
