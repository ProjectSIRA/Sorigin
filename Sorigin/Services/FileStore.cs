using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Sorigin.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sorigin.Services
{
    public interface IFileStore
    {
        Task<FileData> SaveFile(string group, Guid identifier, string fileName, Stream content, Func<string, Task<bool>>? hashComputed = null);
    }

    public class FileStore : IFileStore
    {
        private readonly ILogger _logger;
        private readonly IStreamHasher _streamHasher;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileStore(ILogger<FileStore> logger, IStreamHasher streamHasher, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _streamHasher = streamHasher;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<FileData> SaveFile(string group, Guid identifier, string fileName, Stream content, Func<string, Task<bool>>? hashComputed = null)
        {
            string hash = await _streamHasher.CalculateHashAsync(content);

            bool shouldContinue = true;
            if (hashComputed is not null)
                shouldContinue = await hashComputed.Invoke(hash);
            if (!shouldContinue)
                return new FileData(null, hash);

            DirectoryInfo webRoot = new(_webHostEnvironment.WebRootPath);
            if (!webRoot.Exists)
                webRoot.Create();

            string localPath = Path.Combine(group, identifier.ToString());

            if (!webRoot.GetDirectories(group).FirstOrDefault()?.Exists ?? true)
                webRoot.CreateSubdirectory(group);

            DirectoryInfo groupFolder = webRoot.GetDirectories(localPath).FirstOrDefault() ?? webRoot.CreateSubdirectory(localPath);
            if (!groupFolder.Exists)
                groupFolder.Create();
            _logger.LogInformation("Saving file {File}", fileName);
            string savePath = Path.Combine(group, identifier.ToString(), $"{hash}{Path.GetExtension(fileName)}");

            string fullPath = Path.Combine(webRoot.FullName, savePath);
            if (File.Exists(fullPath))
                return new FileData(fullPath, hash);

            content.Position = 0;
            using FileStream fileStream = File.Create(fullPath);
            await content.CopyToAsync(fileStream);

            return new FileData($"/{savePath.Replace("\\", "/").ToLower()}", hash);
        }
    }
}