namespace Lockdown.Tests.Utils
{
    using System;
    using System.IO;
    using McMaster.Extensions.CommandLineUtils;

#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    public class TestConsole : IConsole
    {
        private readonly MemoryStream outStream;

        public TestConsole()
        {
            this.outStream = new MemoryStream();
            this.Out = new StreamWriter(this.outStream);
        }

#pragma warning disable CS0067 // Unused events

        public event ConsoleCancelEventHandler CancelKeyPress;

        public TextWriter Out
        {
            get;
            private set;
        }

#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
        public TextWriter Error => throw new NotImplementedException();

        public TextReader In => throw new NotImplementedException();

        public bool IsInputRedirected => throw new NotImplementedException();

        public bool IsOutputRedirected => throw new NotImplementedException();

        public bool IsErrorRedirected => throw new NotImplementedException();

        public ConsoleColor ForegroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ConsoleColor BackgroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations

        public string GetWrittenContent()
        {
            this.Out.Flush();
            this.outStream.Flush();
            this.outStream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(this.outStream);
            return reader.ReadToEnd();
        }

        public void ResetColor()
        {
            throw new NotImplementedException();
        }
    }
}
