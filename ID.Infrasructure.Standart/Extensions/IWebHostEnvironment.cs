using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace ID.Infrastructure.Extensions
{
    public interface IWebHostEnvironment : IHostEnvironment
    {
        //
        // Summary:
        //     Gets or sets an Microsoft.Extensions.FileProviders.IFileProvider pointing at
        //     Microsoft.AspNetCore.Hosting.IWebHostEnvironment.WebRootPath.
        IFileProvider WebRootFileProvider { get; set; }
        //
        // Summary:
        //     Gets or sets the absolute path to the directory that contains the web-servable
        //     application content files.
        string WebRootPath { get; set; }
    }
}