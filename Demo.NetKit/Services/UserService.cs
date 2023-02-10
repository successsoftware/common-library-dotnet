using SSS.AspNetCore.Extensions.Jwt.Models;
using SSS.AspNetCore.Extensions.Jwt.Services;
using System.Security.Claims;

namespace Demo.NetKit.Services
{
    public class UserService : IAccountManager
    {
        public async Task<AccountResult> VerifyAccountAsync(string username, string password, TokenRequest tokenRequest)
        {
            // Uses only for demo purpose
            if (!username.Equals("admin") || !password.Equals("admin")) return new AccountResult(new { error = "Invalid user" });

            var id = Guid.NewGuid().ToString();
            tokenRequest.Claims.Add(new CustomClaim(ClaimTypes.NameIdentifier, id));
            tokenRequest.Claims.Add(new CustomClaim(type: ClaimTypes.Name, "Admin"));

            tokenRequest.Responses.Add("userId", id);

            await Task.CompletedTask;

            return new AccountResult(tokenRequest);
        }
    }
}
