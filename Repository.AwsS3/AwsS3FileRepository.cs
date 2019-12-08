using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
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


        public Task<FileRepositoryOperationResult<(Stream stream, string contentType)>> FetchFile(string filePath)
        {
            throw new NotImplementedException();
        }

        public async Task<FileRepositoryOperationResult<bool>> PersistFile(string filePath, Stream fileContent, dynamic metadata)
        {
            try
            {
                _logger.LogInformation($"About to upload resource to path {filePath}");
                using var transferUtility = _amazonS3ClientFactory.GetTransferUtility();
                await transferUtility.UploadAsync(fileContent, _awsS3Settings.BucketName,filePath);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError($"Error encountered on server. Message:'{ex.Message}' when writing an object");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unknown encountered on server. Message:'{ex.Message}' when writing an object");
            }

            return FileRepositoryOperationResult<bool>.Success(true);
        }

        public async Task<FileRepositoryOperationResult<bool>> RemoveFile(string filePath, dynamic metadata)
        {
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
    }
}