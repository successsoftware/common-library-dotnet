using Microsoft.IdentityModel.Tokens;
using SSS.AspNetCore.Extensions.Jwt.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SSS.AspNetCore.Extensions.Jwt.Services
{
    public sealed class TokenService : ITokenService
    {
        private readonly ISet<RefreshToken> _refreshTokens = new HashSet<RefreshToken>();

        public IDictionary<string, object> GenerateToken(TokenRequest dto)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(dto.SecurityKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>();

            foreach (var claim in dto.Claims)
            {
#pragma warning disable CS8604 // Possible null reference argument.
                claims.Add(new Claim(claim.Type, claim.Value?.ToString()));
#pragma warning restore CS8604 // Possible null reference argument.
            }

            string refreshToken = Guid.NewGuid().ToString("n");

            _refreshTokens.Add(new RefreshToken(refreshToken, dto));

            var now = DateTime.UtcNow;

            var token = new JwtSecurityToken(
                issuer: dto.Issuer,
                audience: dto.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(dto.Expiration),
                signingCredentials: creds);

            if (dto.Responses.ContainsKey("accessToken")) dto.Responses.Remove("accessToken");

            if (dto.Responses.ContainsKey("expires")) dto.Responses.Remove("expires");

            if (dto.Responses.ContainsKey("refreshToken")) dto.Responses.Remove("refreshToken");

            dto.Responses.Add("accessToken", new JwtSecurityTokenHandler().WriteToken(token));

            dto.Responses.Add("expires", dto.Expiration.TotalSeconds);

            dto.Responses.Add("refreshToken", refreshToken);

            var keys = dto.Responses.Keys.OrderBy(x => x);

            var dic = new Dictionary<string, object>();

            foreach (string key1 in keys)
            {
                dic.Add(key1, dto.Responses[key1]);
            }

            dic.Add("tokenType", "Bearer");

            return dic;
        }

        public IDictionary<string, object> RefreshAccessToken(string token)
        {
            var refreshToken = GetRefreshToken(token);

            if (refreshToken == null)
            {
                throw new Exception("invalid refresh_token");
            }
            if (refreshToken.Revoked)
            {
                throw new Exception("Refresh token was revoked");
            }

            var tokenRequest = refreshToken.TokenRequest;

            tokenRequest.Expiration = TimeSpan.FromSeconds(+tokenRequest.TotalSeconds);

            return GenerateToken(refreshToken.TokenRequest);
        }

        public void RevokeRefreshToken(string token)
        {
            var refreshToken = GetRefreshToken(token);

            if (refreshToken == null)
            {
                throw new Exception("Refresh token was not found.");
            }
            if (refreshToken.Revoked)
            {
                throw new Exception("Refresh token was already revoked.");
            }

            refreshToken.Revoked = true;
        }

        private RefreshToken GetRefreshToken(string token)
        {
            var refreshToken = _refreshTokens.SingleOrDefault(x => x.Token == token);

            if (refreshToken is null) throw new ArgumentNullException(nameof(refreshToken));

            _refreshTokens.Remove(refreshToken);

            return refreshToken;
        }
    }
}
