using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Repository.AwsS3.Configuration;

namespace Repository.AwsS3
{
    public class AwsS3FileRepository: IFileRepository
    {
        private readonly AmazonS3ClientFactory _amazonS3ClientFactory;
        private readonly ILogger _logger;
        private readonly AwsS3Settings _awsS3Settings;

        public AwsS3FileRepository(
            AmazonS3ClientFactory amazonS3ClientFactory,
                ILogger<AwsS3FileRepository> logger,
                IOptions<AwsS3Settings> options
            )
        {
            _amazonS3ClientFactory = amazonS3ClientFactory;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _awsS3Settings = options.Value;
        }

        public async Task<FileRepositoryOperationResult<(Stream stream, string contentType)>> FetchFile(string filePath)
        {
            _logger.LogInformation($"Fetching resource from path {filePath}");
            filePath = NormalizeFilePath(filePath);
            
            var request = new GetObjectRequest()
            {
                BucketName = _awsS3Settings.BucketName,
                Key = filePath
            };

            try
            {
                var client = _amazonS3ClientFactory.GetClient();
                var response = await client.GetObjectAsync(request);
                var contentType = response.Headers["Content-Type"];
                return FileRepositoryOperationResult<(Stream stream, string contentType)>.Success((response.ResponseStream, contentType));
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.GetType().Name}: {ex.Message}");
                return FileRepositoryOperationResult<(Stream stream, string contentType)>.Error(ex.Message);
            }
        }

        public async Task<FileRepositoryOperationResult<bool>> PersistFile(string filePath, Stream fileContent, dynamic metadata)
        {
            _logger.LogInformation($"Persisting resource to path {filePath}");
            filePath = NormalizeFilePath(filePath);
            try
            {
                using var transferUtility = _amazonS3ClientFactory.GetTransferUtility();
                await transferUtility.UploadAsync(fileContent, _awsS3Settings.BucketName,filePath);
                return FileRepositoryOperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.GetType().Name}: {ex.Message}");
                return FileRepositoryOperationResult<bool>.Error(ex.Message);
            }
        }


        public async Task<FileRepositoryOperationResult<bool>> RemoveFile(string filePath, dynamic metadata)
        {
            _logger.LogInformation($"Removing resource from path {filePath}");
            filePath = NormalizeFilePath(filePath);
            using var client = _amazonS3ClientFactory.GetClient();
            var request = new DeleteObjectRequest
            {
                BucketName = _awsS3Settings.BucketName,
                Key = filePath
            };

            var response = await client.DeleteObjectAsync(request);
            var success = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            return FileRepositoryOperationResult<bool>.Success(success);
        }
        
        private static string NormalizeFilePath(string filePath)
        {
            filePath = string.Join('/', filePath.Split('/', StringSplitOptions.RemoveEmptyEntries));
            return filePath;
        }
    }
}