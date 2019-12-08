using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Repository.AwsS3.Configuration;
using Repository.FileSystem.Configuration;

namespace Api
{
    public class Startup
    {
        
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", 
                    new OpenApiInfo
                    {
                        Title = "file repository wrapper api", Version = "v1"
                    });
            });
            
            ConfigureFileRepository(services);
        }

        private void ConfigureFileRepository(IServiceCollection services)
        {
            var fileRepositoryProvider = _configuration.GetSection("FileRepository:Provider").Value;
            if (string.IsNullOrWhiteSpace(fileRepositoryProvider))
                throw new ApplicationException("Unable to determine repository type");
            if (fileRepositoryProvider.Equals("FileSystem", StringComparison.InvariantCultureIgnoreCase))
            {
                services.AddFileSystem(_configuration);
            }
            else if (fileRepositoryProvider.Equals("AWS", StringComparison.InvariantCultureIgnoreCase))
            {
                services.AddAwsS3(_configuration);
            }
            else
            {
                throw new ApplicationException($"Unknown repository type ({fileRepositoryProvider})");
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "file repository wrapper api");
                c.RoutePrefix = string.Empty;
            });

            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}