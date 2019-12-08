using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Repository.FileSystem
{
    public class FileSystemFileRepository: IFileRepository
    {
        private readonly ILogger _logger;

        public FileSystemFileRepository(ILogger<FileSystemFileRepository> logger)
        {
            _logger = logger;
        }
        
        public Task<FileRepositoryOperationResult<(Stream stream, string contentType)>> FetchFile(string filePath)
        {
            _logger.LogInformation($"Fetching resource from path {filePath}");
            
            if (string.IsNullOrWhiteSpace(filePath)) 
                throw new ArgumentNullException(nameof(filePath));

            try
            {
                filePath = NormalizeFilePath(filePath);
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return Task.FromResult(FileRepositoryOperationResult<(Stream stream, string contentType)>.Success((fileStream, "application/pdf")));
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.GetType().Name}: {ex.Message}");
                return Task.FromResult(FileRepositoryOperationResult<(Stream stream, string contentType)>.Error(ex.Message));
            }
        }

        public async Task<FileRepositoryOperationResult<bool>> PersistFile(string filePath, Stream fileContent, dynamic metadata)
        {
            _logger.LogInformation($"Persisting resource to path {filePath}");

            if (string.IsNullOrWhiteSpace(filePath)) 
                throw new ArgumentNullException(nameof(filePath));
            
            if (fileContent == null) 
                throw new ArgumentNullException(nameof(fileContent));
            
            try
            {
                filePath = NormalizeFilePath(filePath);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                await using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                fileContent.Seek(0, SeekOrigin.Begin);
                await fileContent.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.GetType().Name}: {ex.Message}");
                return FileRepositoryOperationResult<bool>.Error(ex.Message);
            }
            return FileRepositoryOperationResult<bool>.Success(true);
        }

        
        private static string NormalizeFilePath(string filePath)
        {
            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var relativePath = Path.Combine(filePath.Split('/', StringSplitOptions.RemoveEmptyEntries).ToArray());
            return Path.Combine(basePath, "Data", relativePath);
        }
        
        public  Task<FileRepositoryOperationResult<bool>> RemoveFile(string filePath, dynamic metadata)
        {
            _logger.LogInformation($"Removing resource from path {filePath}");
            
            if (string.IsNullOrWhiteSpace(filePath)) 
                throw new ArgumentNullException(nameof(filePath));

            try
            {
                filePath = NormalizeFilePath(filePath);
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.GetType().Name}: {ex.Message}");
                return  Task.FromResult(FileRepositoryOperationResult<bool>.Error(ex.Message));
            }
            return Task.FromResult(FileRepositoryOperationResult<bool>.Success(true));
        }
    }
}