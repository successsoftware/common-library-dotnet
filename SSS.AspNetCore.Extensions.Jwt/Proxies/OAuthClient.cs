using Newtonsoft.Json;
using SSS.AspNetCore.Extensions.Jwt.Models;
using System.Net;

namespace SSS.AspNetCore.Extensions.Jwt.Proxies
{
    public class OAuthClient
    {
        private readonly HttpClient _httpClient;

        public OAuthClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<JwtResult> EnsureApiTokenAsync(string username, string password, string tokenServerPath = "/token")
        {
            HttpResponseMessage response = await _httpClient.PostAsync(tokenServerPath, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", username },
                    { "password", password }
                }));

            var jsonStr = await response.Content.ReadAsStringAsync();

            var jsonObj = JsonConvert.DeserializeObject<object>(jsonStr);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return new JwtResult(true, jsonObj);
            }
            else
            {
                return JwtResult.Failure(jsonObj);
            }
        }

        public async Task<JwtResult> RefreshTokenAsync(string refreshTokenId, string tokenServerPath = "/token")
        {
            HttpResponseMessage response = await _httpClient.PostAsync(tokenServerPath, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "refreshToken" },
                    { "refresh_token", refreshTokenId }
                }));

            var jsonStr = await response.Content.ReadAsStringAsync();

            var jsonObj = JsonConvert.DeserializeObject<object>(jsonStr);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return new JwtResult(true, jsonObj);
            }
            else
            {
                return JwtResult.Failure(jsonObj);
            }
        }
    }
}
