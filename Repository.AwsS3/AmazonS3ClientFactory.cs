using System;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using Repository.AwsS3.Configuration;

namespace Repository.AwsS3
{
    public class AmazonS3ClientFactory
    {
        private readonly IOptions<AwsS3Settings> _options;

        public AmazonS3ClientFactory(IOptions<AwsS3Settings> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        
        public AmazonS3Client GetClient()
        {
            var awsAccessKeyId = FetchAccessKeyId();
            var awsAccessSecretKey = FetchAccessSecretKey();
            var regionEndPoint = RegionEndPoint();
            return new 
                AmazonS3Client(
                    awsAccessKeyId, 
                    awsAccessSecretKey,
                    regionEndPoint);
        }

        public TransferUtility GetTransferUtility()
        {
            return 
                new TransferUtility(GetClient());
        }

        private string FetchAccessKeyId()
        {
            var awsAccessKeyId = _options.Value.AccessKeyId;
            if (string.IsNullOrWhiteSpace(awsAccessKeyId))
            {
                throw new ArgumentException("AWS Access Key Id cannot be empty.", nameof(awsAccessKeyId));
            }

            return awsAccessKeyId;
        }

        private string FetchAccessSecretKey()
        {
            var awsAccessSecretKey = _options.Value.AccessSecretKey;
            if (string.IsNullOrWhiteSpace(awsAccessSecretKey))
            {
                throw new ArgumentException("AWS Access Secret Key cannot be empty.", nameof(awsAccessSecretKey));
            }
            return awsAccessSecretKey;
        }
        
        private RegionEndpoint RegionEndPoint()
        {
            var regionEndPoint = Amazon.RegionEndpoint.GetBySystemName(_options.Value.RegionSystemName);
            if (regionEndPoint == null)
            {
                throw new ArgumentException("Cannot determine aws region", nameof(regionEndPoint));
            }

            return regionEndPoint;
        }
    }
}