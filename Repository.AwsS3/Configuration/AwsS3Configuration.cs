using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Repository.AwsS3.Configuration
{
    public static class AwsS3Configuration
    {
        public static IServiceCollection AddAwsS3(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IFileRepository, AwsS3FileRepository>();
            services.AddTransient<AmazonS3ClientFactory>();
            services.Configure<AwsS3Settings>(configuration.GetSection("FileRepository:Settings"));
            return services;
        }

    }
}