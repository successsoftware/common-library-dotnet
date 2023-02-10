using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace SSS.AspNetCore.Extensions.Jwt
{
    public interface IClaimsProvider<TOptions> where TOptions : AuthenticationSchemeOptions
    {
        Task<ICollection<Claim>> GetClaims(ResultContext<TOptions> context);
    }

    public interface IJwtClaimsProvider : IClaimsProvider<JwtBearerOptions>
    { }
}