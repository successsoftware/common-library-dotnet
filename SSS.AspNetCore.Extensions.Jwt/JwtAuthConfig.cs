using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace SSS.AspNetCore.Extensions.Jwt
{
    public static class JwtAuthConfig
    {
        public static AuthenticationBuilder AddBearerAuth(this AuthenticationBuilder services, JwtAppAuth jwtApp)
            => services.AddBearerAuth<DefaultJwtBearerEvents>(jwtApp);

        public static AuthenticationBuilder AddBearerAuth(this AuthenticationBuilder services, string scheme, JwtAppAuth jwtApp)
            => services.AddBearerAuth<DefaultJwtBearerEvents>(scheme, jwtApp);

        public static AuthenticationBuilder AddBearerAuth<TEvents>(this AuthenticationBuilder services, JwtAppAuth jwtApp) where TEvents : DefaultJwtBearerEvents
            => services.AddBearerAuth<TEvents>(JwtBearerDefaults.AuthenticationScheme, jwtApp);

        /// <summary>
        /// Add with custom TEvents
        /// </summary>
        /// <typeparam name="TEvents"></typeparam>
        /// <param name="services"></param>
        /// <param name="azApp"></param>
        /// <returns></returns>
        public static AuthenticationBuilder AddBearerAuth<TEvents>(this AuthenticationBuilder services, string scheme, JwtAppAuth azApp) where TEvents : DefaultJwtBearerEvents
        {
            services.Services.AddSingleton<TEvents>();
            services.AddJwtBearer(scheme, options =>
             {
                 options.SaveToken = azApp.OverrideOptions?.SaveToken ?? false;
                 options.RequireHttpsMetadata = azApp.OverrideOptions?.RequireHttpsMetadata ?? true;
                 options.Authority = azApp.Authority;
                 options.Audience = azApp.ClientId;

                 options.EventsType = typeof(TEvents);

                 options.TokenValidationParameters = new TokenValidationParameters()
                 {
                     SignatureValidator = (t, p) => new JwtSecurityToken(t),

                     RequireAudience = azApp.OverrideOptions?.RequireAudience ?? true,
                     RequireExpirationTime = azApp.OverrideOptions?.RequireExpirationTime ?? true,
                     RequireSignedTokens = azApp.OverrideOptions?.RequireSignedTokens ?? true,

                     ValidateIssuer = azApp.OverrideOptions?.ValidateIssuer ?? azApp.Issuers?.Count > 0,
                     ValidIssuers = azApp.Issuers,

                     ValidAudiences = azApp.Audiences,
                     ValidateAudience = azApp.OverrideOptions?.ValidateAudience ?? true,
                     ValidateLifetime = azApp.OverrideOptions?.ValidateLifetime ?? true
                 };
             });

            return services;
        }
    }
}