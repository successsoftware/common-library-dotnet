namespace SSS.AspNetCore.Extensions.Jwt
{
    public class JwtAppAuth
    {
        public IList<string> Audiences { get; set; } = new List<string>();

        public string Authority { get; set; }

        public string ClientCert { get; set; }

        public string ClientCertPass { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public bool IsB2cApp { get; set; }

        public ICollection<string> Issuers { get; set; }

        public string Name { get; set; }

        public AuthOptions OverrideOptions { get; set; }

        public string Tenant { get; set; }

        public string TenantId { get; set; }
    }
}