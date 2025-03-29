using CompressedStaticFiles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;

namespace Floorplan3D.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services
                .AddCompressedStaticFiles();

            // Build the app
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            var contentTypeProvider = new FileExtensionContentTypeProvider();

            var evergineExtensions = new[]
            {
                ".weptx", ".wepsn", ".wepsc", ".wepsp", ".weprl", ".weprp", ".weppp", ".wepmd", ".wepmt", ".wepfb",
                ".wepfx",
                ".wepprf",
            };
            foreach (var evergineExtension in evergineExtensions)
            {
                contentTypeProvider.Mappings.Add(evergineExtension, "application/octet-stream");
            }

            app
                .UseHttpsRedirection()
                .UseRouting()
                .UseStaticFiles()
                .UseCompressedStaticFiles(
                    new StaticFileOptions
                    {
                        ServeUnknownFileTypes = true,
                        ContentTypeProvider = contentTypeProvider,
                    });
            app.MapFallbackToFile("index.html");

            // Run the app
            app.Run();
        }
    }
}

