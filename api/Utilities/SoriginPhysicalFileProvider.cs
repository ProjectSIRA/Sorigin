using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;

namespace Sorigin.Utilities;

public class SoriginPhysicalFileProvider : IImageProvider
{
    private readonly IFileProvider _fileProvider;
    private readonly FormatUtilities _formatUtilities;

    public Func<HttpContext, bool> Match { get; set; } = _ => true;
    public ProcessingBehavior ProcessingBehavior { get; } = ProcessingBehavior.CommandOnly;

    public SoriginPhysicalFileProvider(IWebHostEnvironment environment, FormatUtilities formatUtilities)
    {
        _fileProvider = environment.WebRootFileProvider;
        _formatUtilities = formatUtilities;
    }

    public bool IsValidRequest(HttpContext context)
    {
        var url = context.Request.GetDisplayUrl();
        bool notNull = _formatUtilities.GetExtensionFromUri(url) != null;
        return notNull;
    }

    public Task<IImageResolver> GetAsync(HttpContext context)
    {
        IFileInfo fileInfo = this._fileProvider.GetFileInfo(context.Request.Path.Value!.Replace("/cdn", string.Empty));
        if (!fileInfo.Exists)
        {
            return Task.FromResult<IImageResolver>(null!);
        }
        ImageMetadata metadata = new(fileInfo.LastModified.UtcDateTime, fileInfo.Length);
        return Task.FromResult<IImageResolver>(new PhysicalFileSystemResolver(fileInfo, metadata));
    }
}