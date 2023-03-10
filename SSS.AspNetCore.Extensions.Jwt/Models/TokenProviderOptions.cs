namespace SSS.AspNetCore.Extensions.Jwt.Models
{
    public class TokenProviderOptions
    {
        public string Path { get; set; } = JwtSettings.DefaultPath;

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public TimeSpan Expiration { get; set; } = TimeSpan.FromDays(+1);

        public string SecurityKey { get; set; } = JwtSettings.DefaultSecretKey;
    }
}