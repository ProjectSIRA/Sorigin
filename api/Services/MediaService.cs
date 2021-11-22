using Microsoft.EntityFrameworkCore;
using MimeKit;
using NodaTime;
using Sorigin.Models;

namespace Sorigin.Services;

public interface IMediaService
{
    Task<Media> Upload(string group, string subGroup, string fileName, Stream stream, string contract);
    Task<bool> Delete(Media media);
}

internal class MediaService : IMediaService
{
    private readonly IClock _clock;
    private readonly ILogger _logger;
    private readonly IFileService _fileService;
    private readonly SoriginContext _soriginContext;

    public MediaService(IClock clock, ILogger logger, IFileService fileService, SoriginContext soriginContext)
    {
        _clock = clock;
        _logger = logger;
        _fileService = fileService;
        _soriginContext = soriginContext;
    }

    public async Task<Media> Upload(string group, string subGroup, string fileName, Stream stream, string contract)
    {
        Guid mediaID = Guid.NewGuid();
        _logger.LogInformation("Uploading a new media icon with the ID {ID}", mediaID);
        FileData fileData = await _fileService.SaveFile(group, subGroup, fileName, stream, Affirm);

        if (fileData.Path is null)
            return await _soriginContext.Media.FirstAsync(m => m.FileHash == fileData.Hash);

        Media media = new()
        {
            ID = mediaID,
            FileSize = stream.Length,
            FileHash = fileData.Hash,
            Uploaded = _clock.GetCurrentInstant(),
            MimeType = MimeTypes.GetMimeType(fileName),
            Path = fileData.Path,
            Contract = contract
        };

        _soriginContext.Media.Add(media);
        await _soriginContext.SaveChangesAsync();
        return media;
    }

    private async Task<bool> Affirm(string hash)
    {
        // We check to see if we already have a media element with the same hash. If so, lets reuse it.
        Media? media = await _soriginContext.Media.FirstOrDefaultAsync(m => m.FileHash == hash);
        return media is null;
    }

    public async Task<bool> Delete(Media media)
    {
        try
        {
            _soriginContext.Media.Remove(media);
            await _soriginContext.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}