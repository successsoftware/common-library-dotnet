using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace SSS.AspNetCore.Extensions.Jwt
{
    public class DefaultJwtBearerEvents : JwtBearerEvents
    {
        public DefaultJwtBearerEvents(IEnumerable<IClaimsProvider<JwtBearerOptions>> claimsProviders) => ClaimsProviders = claimsProviders.ToList();

        private IReadOnlyCollection<IClaimsProvider<JwtBearerOptions>> ClaimsProviders { get; }

        public override Task MessageReceived(MessageReceivedContext context)
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs", StringComparison.CurrentCultureIgnoreCase))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }

            return base.MessageReceived(context);
        }

        public override async Task TokenValidated(TokenValidatedContext context)
        {
            var claims = new List<Claim>();

            if (ClaimsProviders.Any())
            {
                var results = await Task.WhenAll(ClaimsProviders.Select(p => p.GetClaims(context)));
                claims.AddRange(results.SelectMany(c => c));
            }

            if (claims.Any())
                context.Principal?.AddIdentity(new ClaimsIdentity(claims.Distinct()));

            await base.TokenValidated(context);
        }
    }
}