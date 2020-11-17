namespace Lockdown.Run
{
    
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Hosting;
    using System.IO;

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {

            app.UseFileServer(new FileServerOptions()
            
            
            {

                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "_site"))
            }
            );
        }
    }
}
