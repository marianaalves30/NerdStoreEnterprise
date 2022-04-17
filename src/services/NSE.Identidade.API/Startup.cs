using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSE.Identidade.API.Configuration;

namespace NSE.Identidade.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IHostEnvironment hostEnvironment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(hostEnvironment.ContentRootPath) //pega o caminho da onde a aplica��o t�
                .AddJsonFile("appsettings.json", true, true) //descobre onde t� o arquivo json 
                .AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.json", true, true) 
                .AddEnvironmentVariables();

            if (hostEnvironment.IsDevelopment()) //se for o ambiente de desenvolvimento ele adicionar� o secrets
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityConfiguration(Configuration);
            services.AddApiConfiguration();
            services.AddSwaggerConfiguration();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwaggerConfiguration();
            app.UseApiConfiguration(env);
        }
    }
}
