using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Globalization;

namespace NL2SQLBotFrameworkBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
            });

            // Authentication configuration with allowed callers and valid token issuers
            services.AddSingleton(sp =>
            {
                var config = sp.GetService<IConfiguration>();

                var allowedCallers = config.GetSection("AllowedCallers").Get<List<string>>();
                var claimsValidator = new AllowedCallersClaimsValidator(allowedCallers);

                var validTokenIssuers = new List<string>();
                var tenantId = config.GetValue<string>(MicrosoftAppCredentials.MicrosoftAppTenantIdKey);

                if (!string.IsNullOrWhiteSpace(tenantId))
                {
                    validTokenIssuers.Add(string.Format(CultureInfo.InvariantCulture, AuthenticationConstants.ValidTokenIssuerUrlTemplateV1, tenantId));
                    validTokenIssuers.Add(string.Format(CultureInfo.InvariantCulture, AuthenticationConstants.ValidTokenIssuerUrlTemplateV2, tenantId));
                    validTokenIssuers.Add(string.Format(CultureInfo.InvariantCulture, AuthenticationConstants.ValidGovernmentTokenIssuerUrlTemplateV1, tenantId));
                    validTokenIssuers.Add(string.Format(CultureInfo.InvariantCulture, AuthenticationConstants.ValidGovernmentTokenIssuerUrlTemplateV2, tenantId));
                }

                return new AuthenticationConfiguration
                {
                    ClaimsValidator = claimsValidator,
                    ValidTokenIssuers = validTokenIssuers
                };
            });

            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.AddTransient<IBot, Bots.EchoBot>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
