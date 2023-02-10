namespace SSS.AspNetCore.Extensions.Jwt
{
    public class JwtAuthSetting
    {
        public static string Name => "AuthConfigs";

        public IList<JwtAppAuth> Apps { get; } = new List<JwtAppAuth>();

        public virtual string Authority
            => string.IsNullOrWhiteSpace(SignInPolicy) ? $"{BaseUrl}/{Tenant ?? TenantId}/" : $"{BaseUrl}/tfp/{Tenant ?? TenantId}/{SignInPolicy}/{Version}";

        public virtual string BaseUrl { get; set; } = "https://login.microsoftonline.com";

        public virtual IList<string> Issuers => new List<string> { Authority, $"{BaseUrl}/{TenantId ?? Tenant}/", $"{BaseUrl}/tfp/{TenantId ?? Tenant}/${Version}/" };

        public string SignInPolicy { get; set; }

        public string Tenant { get; set; }

        public string TenantId { get; set; }

        public string Version { get; set; } = "v2.0";

        public virtual JwtAppAuth GetApp(string name)
        {
            var config = Apps.FirstOrDefault(a => a.Name.EndsWith(name, StringComparison.CurrentCultureIgnoreCase));

            if (config is null) throw new ArgumentNullException(nameof(config));

            config.Authority = Authority;
            config.Issuers = Issuers;
            config.Tenant = Tenant;
            config.TenantId = TenantId;
            config.IsB2cApp = !string.IsNullOrEmpty(SignInPolicy);

            if (!config.Audiences.Contains(config.ClientId))
                config.Audiences.Add(config.ClientId);

            return config;
        }
    }
}