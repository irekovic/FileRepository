using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Repository.FileSystem.Configuration
{
    public static class FileSystemConfiguration
    {
        public static IServiceCollection AddFileSystem(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IFileRepository, FileSystemFileRepository>();
            return services;
        }
    }
}