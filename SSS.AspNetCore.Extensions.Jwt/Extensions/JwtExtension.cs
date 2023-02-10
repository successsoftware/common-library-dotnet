using IdentityModel;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SSS.AspNetCore.Extensions.Jwt.Middleware;
using SSS.AspNetCore.Extensions.Jwt.Models;
using SSS.AspNetCore.Extensions.Jwt.Proxies;
using SSS.AspNetCore.Extensions.Jwt.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace SSS.AspNetCore.Extensions.Jwt
{
    public static class JwtExtension
    {
        public static IApplicationBuilder UseJwtBearerTokenMiddleware(this IApplicationBuilder app,
            IConfiguration configuration, TokenProviderOptions tokenProviderOptions = null, int tokenLifetime = 1440)
        {
            JwtOptions jwtOptions = new();
            configuration.Bind(JwtOptions.Name, jwtOptions);

            var secretKey = string.IsNullOrEmpty(jwtOptions.SecurityKey)
                ? JwtSettings.DefaultSecretKey
                : jwtOptions.SecurityKey;

            var tokenProviderOptionsOpt = tokenProviderOptions ?? new TokenProviderOptions
            {
                Path = JwtSettings.DefaultPath,
                SecurityKey = secretKey,
                Expiration = TimeSpan.FromMinutes(tokenLifetime),
                Audience = jwtOptions.Audience,
                Issuer = jwtOptions.Issuer
            };

            app.UseMiddleware<TokenProviderMiddleware>(tokenProviderOptionsOpt);

            return app;
        }

        public static IServiceCollection AddJwtBearerToken(this IServiceCollection services,
            IConfiguration configuration,
            string defaultScheme = JwtBearerDefaults.AuthenticationScheme,
            Action<AuthenticationOptions> authenticationOptions = null,
            TokenValidationParameters tokenValidationParameters = null,
            Action<JwtBearerOptions> jwtBearerOptions = null)
        {
            services.AddJwtTokenServices(configuration);

            JwtOptions jwtOptions = new();
            configuration.Bind(JwtOptions.Name, jwtOptions);

            var secretKey = string.IsNullOrEmpty(jwtOptions.SecurityKey)
                ? JwtSettings.DefaultSecretKey
                : jwtOptions.SecurityKey;

            AuthenticationBuilder builder;

            builder = authenticationOptions == null ? services.AddAuthentication(defaultScheme) : services.AddAuthentication(authenticationOptions);

            builder.AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters ?? new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };

                jwtBearerOptions?.Invoke(options);
            });

            return services;
        }

        public static IServiceCollection AddIdsr4Authentication(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddAuthentication()
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = configuration.GetValue<string>("Idsr4:IssuerUri");
                    options.ApiName = configuration.GetValue<string>("Idsr4:Scope");
                    options.RequireHttpsMetadata = false;
                    options.SupportedTokens = SupportedTokens.Both;
                });

            return services;
        }

        public static IServiceCollection AddIdsr4JwtBearer(this IServiceCollection services,
            IConfiguration configuration,
            Action<JwtBearerOptions> configureOptions = null)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration.GetValue<string>("Idsr4:IssuerUri");
                    options.Audience = configuration.GetValue<string>("Idsr4:Scope");
                    options.RequireHttpsMetadata = false;

                    if (configureOptions != null)
                    {
                        configureOptions.Invoke(options);
                    }
                });

            return services;
        }

        public static IServiceCollection AddIdsr4JwtBearerForTest(this IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            return services;
        }

        public static IServiceCollection AddIdsr4OpenIdConnectForTest(this IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential 
                // cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                // requires using Microsoft.AspNetCore.Http;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            return services;
        }

        public static IServiceCollection AddIdsr4OpenIdConnect(this IServiceCollection services, IConfiguration configuration, Action<OpenIdConnectOptions> configureOptions = null)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            var scopes = configuration.GetValue<string>("Idsr4:Scope").Split(' ');

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            }).AddCookie("Cookies")
             .AddOpenIdConnect("oidc", options =>
             {
                 options.CorrelationCookie = new CookieBuilder
                 {
                     SameSite = SameSiteMode.None,
                     SecurePolicy = CookieSecurePolicy.Always,
                     HttpOnly = true
                 };

                 options.ProtocolValidator.RequireNonce = false;

                 options.SignInScheme = "Cookies";

                 options.Authority = configuration.GetValue<string>("Idsr4:IssuerUri");

                 options.RequireHttpsMetadata = false;

                 options.ClientId = configuration.GetValue<string>("Idsr4:ClientId");

                 options.ClientSecret = configuration.GetValue<string>("Idsr4:Secret");

                 options.ResponseType = "code";

                 options.SaveTokens = true;

                 foreach (var scope in scopes)
                 {
                     options.Scope.Add(scope);
                 }

                 if (configureOptions != null) configureOptions.Invoke(options);
             });

            return services;
        }

        private static void AddJwtTokenServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<JwtOptions>().Bind(configuration.GetSection(JwtOptions.Name));

            JwtOptions jwtOptions = new();
            configuration.Bind(JwtOptions.Name, jwtOptions);

            services.AddHttpClient<OAuthClient>(nameof(OAuthClient),
                client => client.BaseAddress = new Uri(jwtOptions.Issuer));

            services.Scan(scan =>
            {
                scan.FromEntryAssembly()
                    .AddClasses(classes => classes.AssignableTo<IAccountManager>())
                    .AsImplementedInterfaces()
                    .WithScopedLifetime();
            });

            services.AddSingleton<ITokenService, TokenService>();
        }
    }

    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _configuration;

        public static Action<List<Claim>> onCustomClaim;

        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, IConfiguration configuration)
            : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new List<Claim> {
                new Claim(JwtClaimTypes.Subject, _configuration.GetValue<string>("Claim:Subject")),
                new Claim(JwtClaimTypes.Name, _configuration.GetValue<string>("Claim:Name")),
                new Claim(JwtClaimTypes.Role, _configuration.GetValue<string>("Claim:Role")),
                new Claim("role_id", _configuration.GetValue<string>("Claim:RoleId")),
                new Claim(JwtClaimTypes.Email, _configuration.GetValue<string>("Claim:Email"))
            };

            if (onCustomClaim != null)
            {
                onCustomClaim(claims);
            }

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }
}