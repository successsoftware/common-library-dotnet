using SSS.AspNetCore.Extensions.Jwt.Models;

namespace SSS.AspNetCore.Extensions.Jwt.Services
{
    public interface IAccountManager
    {
        Task<AccountResult> VerifyAccountAsync(string username,
            string password,
            TokenRequest tokenRequest);
    }
}
