namespace Lockdown.Run
{
    using System.IO;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.FileProviders;

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseFileServer(new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(
                        Path.Combine(Directory.GetCurrentDirectory(), "_site")),
            });
        }
    }
}
