using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace SSS.AspNetCore.Extensions.Jwt
{
    public static class AuthConfig
    {
        public static AuthenticationBuilder AddAuth(this IServiceCollection services, string schema,
            Action<AuthorizationOptions> authOptions = null)
            => services.AddAuthorizationPolicy(schema, (s, o) => { authOptions?.Invoke(o); })
                .AddAuthentication(schema);

        public static IServiceCollection AddClaimsProvider<TProvider, TOptions>(this IServiceCollection services)
            where TProvider : class, IClaimsProvider<TOptions>
            where TOptions : AuthenticationSchemeOptions
            => services.AddSingleton<IClaimsProvider<TOptions>, TProvider>();

        public static IServiceCollection AddClaimsProvider<TProvider>(this IServiceCollection services)
            where TProvider : class, IJwtClaimsProvider
            => services.AddSingleton<IClaimsProvider<JwtBearerOptions>, TProvider>();

        public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration,
            string appName = null, Action<AuthorizationOptions> authOptions = null)
            => services.AddJwtAuth<DefaultJwtBearerEvents>(configuration, appName, authOptions);

        public static IServiceCollection AddJwtAuth<TEvents>(this IServiceCollection services,
            IConfiguration configuration, string appName = null, Action<AuthorizationOptions> authOptions = null)
            where TEvents : DefaultJwtBearerEvents
        {
            JwtAuthSetting setting = new();
            configuration.Bind(JwtAuthSetting.Name, setting);

            switch (setting.Apps.Count)
            {
                case <= 0:
                    throw new ArgumentException($"There is no app found in {JwtAuthSetting.Name} setting.");
                case > 1 when string.IsNullOrEmpty(appName):
                    throw new ArgumentNullException(nameof(appName));
                default:
                    {
                        var app = setting.Apps.Single(a => a.Name.Equals(appName, StringComparison.OrdinalIgnoreCase));

                        return services.AddJwtAuth<TEvents>(app, authOptions);
                    }
            }
        }

        public static IServiceCollection AddJwtAuth(this IServiceCollection services, JwtAppAuth appSetting,
            Action<AuthorizationOptions> authOptions = null)
            => services.AddJwtAuth<DefaultJwtBearerEvents>(appSetting, authOptions);

        public static IServiceCollection AddJwtAuth<TEvents>(this IServiceCollection services, JwtAppAuth appSetting,
            Action<AuthorizationOptions> authOptions = null) where TEvents : DefaultJwtBearerEvents
        {
            services
                .AddAuth(JwtBearerDefaults.AuthenticationScheme, authOptions)
                .AddBearerAuth<TEvents>(appSetting);

            return services;
        }

        public static AuthenticationBuilder AddMultiAuths(this IServiceCollection services, string[] schemas,
            Func<HttpContext, string> forwardDefaultSelector, Action<string, AuthorizationOptions> authOptions)
        {
            var schemaName = string.Join("-", schemas);

            var builder = services.AddAuthorizationPolicy(schemaName, authOptions)
                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = schemaName;
                    sharedOptions.DefaultChallengeScheme = schemaName;
                }).AddPolicyScheme(schemaName, "Switching between schema",
                    options => { options.ForwardDefaultSelector = forwardDefaultSelector; });

            return builder;
        }

        public static IApplicationBuilder UseAuth(this IApplicationBuilder app)
        {
            app.UseAuthentication()
                .UseAuthorization();

            return app;
        }

        public static string GetClaimValue(this IEnumerable<Claim> claims, string claimType)
        {
            var claim = claims.FirstOrDefault(p => p.Type == claimType);

            if (claim == null) return string.Empty;

            return claim.Value;
        }

        private static IServiceCollection AddAuthorizationPolicy(this IServiceCollection services, string schema,
            Action<string, AuthorizationOptions> authOptions)
            => services.AddAuthorization(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(schema)
                    .RequireAuthenticatedUser()
                    .Build();

                options.AddPolicy($"{schema}-Policy", policy);

                authOptions?.Invoke(schema, options);
            });
    }
}