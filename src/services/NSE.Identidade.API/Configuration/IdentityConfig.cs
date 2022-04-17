using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NSE.Identidade.API.Data;
using NSE.Identidade.API.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSE.Identidade.API.Configuration
{
    public static class IdentityConfig
    {
        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            //Adiciona um context do Entity passando as options (pra utilizar o suporte do SqlServer) e nosso arquivo de configuração appsettings.son
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddErrorDescriber<IdentityMensagensPortugues>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // JWT
            var appSettingsSection = configuration.GetSection("AppSettings"); //Acessa o arq de configuração e pega o nó AppSettings
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
            }); ;

            return services;
        }

        public static IApplicationBuilder UseIdentityConfiguration(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
            
        }
    }
}
