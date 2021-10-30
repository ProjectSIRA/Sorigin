using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using NodaTime;
using Sorigin.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sorigin.Services
{
    public interface IMediaService
    {
        Task<Media> Upload(string fileName, Stream stream, string contract);
        Task<bool> Delete(Media media);
    }

    public class MediaService : IMediaService
    {
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly IFileStore _fileStore;
        private readonly SoriginContext _soriginContext;

        public MediaService(IClock clock, ILogger<MediaService> logger, IFileStore fileStore, SoriginContext soriginContext)
        {
            _clock = clock;
            _logger = logger;
            _fileStore = fileStore;
            _soriginContext = soriginContext;
        }

        public async Task<Media> Upload(string fileName, Stream stream, string contract)
        {
            Guid mediaID = Guid.NewGuid();
            FileData fileData = await _fileStore.SaveFile(nameof(Media).ToLower(), mediaID, fileName, stream, Affirm);

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
}
