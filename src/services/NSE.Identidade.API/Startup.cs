using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSE.Identidade.API.Data;
using Microsoft.OpenApi.Models;
using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NSE.Identidade.API.Extensions;

namespace NSE.Identidade.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Adiciona um context do Entity passando as options (pra utilizar o suporte do SqlServer) e nosso arquivo de configuração appsettings.son
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
     
            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // JWT
            var appSettingsSection = Configuration.GetSection("AppSettings"); //Acessa o arq de configuração e pega o nó AppSettings
            services.Configure<AppSettings>(appSettingsSection); //Configura para que a classe appSettings represente os dados da sessão appSettingsSection

            var appSettings = appSettingsSection.Get<AppSettings>(); //Representa a classe AppSettings
            var key = Encoding.ASCII.GetBytes(appSettings.Secret); //Chave transformada em sequencia de bytes para que futuramente seja utilizada na IssuerSigninKey


            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions => // Adionando suporte pra esse tipo de token
            {
                bearerOptions.RequireHttpsMetadata = true; //Requer acesso HTTPS
                bearerOptions.SaveToken = true; //Token vai ser guardado na instância assim que o login for realizado com sucesso
                bearerOptions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters //Param. de validação do token
                {
                    ValidateIssuerSigningKey = true, //Valida o emissor com base na assinatura 
                    IssuerSigningKey = new SymmetricSecurityKey(key), //A assinatura do emissor vai ser criada onde terá uma seq de bytes criando chave de criptografia
                    ValidateIssuer = true, //Valida o emissor
                    ValidateAudience = true, //Valida pra onde esse token é válido
                    ValidAudience = appSettings.ValidoEm,
                    ValidIssuer = appSettings.Emissor //Cria um emissor valido
                };
             });;

            services.AddControllers();

            services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "NerdStore Enterprise Identity API",
                    Description = "Esta API faz parte do curso de ASP .NET Core Enterprise Applications",
                    Contact = new OpenApiContact() { Name = "Mariana Alves", Email = "marianaalves@ufu.br" },
                    License = new OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }

                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(x =>
            {
                x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
