using SSS.AspNetCore.Extensions.Jwt.Models;

namespace SSS.AspNetCore.Extensions.Jwt.Services
{
    public interface ITokenService
    {
        IDictionary<string, object> GenerateToken(TokenRequest dto);

        IDictionary<string, object> RefreshAccessToken(string token);

        void RevokeRefreshToken(string token);

    }
}
