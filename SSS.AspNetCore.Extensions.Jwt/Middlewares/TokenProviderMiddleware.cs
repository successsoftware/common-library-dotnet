using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SSS.AspNetCore.Extensions.Jwt.Enums;
using SSS.AspNetCore.Extensions.Jwt.Models;
using SSS.AspNetCore.Extensions.Jwt.Services;
using System.Text.Json;

namespace SSS.AspNetCore.Extensions.Jwt.Middleware
{
    public class TokenProviderMiddleware
    {
        public readonly RequestDelegate _next;

        public readonly TokenProviderOptions _options;

        public TokenProviderMiddleware(RequestDelegate next, TokenProviderOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Method.Equals("POST") || !context.Request.HasFormContentType)
            {
                await BadRequest(context, new { error = "Bad request.." });

                return;
            }

            GrantTypeEnum grantType;

            try
            {
                grantType = (GrantTypeEnum)Enum.Parse(typeof(GrantTypeEnum), context.Request.Form["grant_type"], true);
            }
            catch
            {
                await BadRequest(context, new { error = "invail grant_type" });

                return;
            }

            switch (grantType)
            {
                case GrantTypeEnum.Password:
                    await GenerateTokenByUserNamePassWord(context, context.Request.Form["username"],
                        context.Request.Form["password"]);
                    break;
                case GrantTypeEnum.RefreshToken:
                    await GenerateTokenByRefreshToken(context, context.Request.Form["refresh_token"]);
                    break;
            }
        }

        #region PRIVATE METHOD

        private async Task GenerateTokenByUserNamePassWord(HttpContext context,
            string username,
            string password)
        {
            var customClaims = new List<CustomClaim>
            {
                new CustomClaim("iss", _options.Issuer)
            };

            var tokenRequest = new TokenRequest(_options, customClaims);

            IServiceProvider serviceProvider = context.RequestServices;

            var accountManager = serviceProvider.GetService<IAccountManager>();

            if (accountManager is null) throw new ArgumentException(nameof(accountManager));

            var accountResult = await accountManager.VerifyAccountAsync(username, password, tokenRequest);

            if (accountResult.Successed)
            {
                context.Response.StatusCode = 200;

                context.Response.ContentType = "application/json";

                var tokenService = serviceProvider.GetService<ITokenService>();

                if (tokenService is null) throw new ArgumentException(nameof(tokenService));

#pragma warning disable CS8604 // Possible null reference argument for parameter 'dto' in 'IDictionary<string, object> ITokenService.GenerateToken(TokenRequest dto)'.
                var token = tokenService.GenerateToken(accountResult.TokenRequest);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'dto' in 'IDictionary<string, object> ITokenService.GenerateToken(TokenRequest dto)'.

                await context.Response.WriteAsync(ObjToJson(token));

                return;
            }
            else
            {
                context.Response.StatusCode = 400;

                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(ObjToJson(accountResult.Error));

                return;
            }
        }

        private static async Task GenerateTokenByRefreshToken(HttpContext context, string refreshToken)
        {
            try
            {
                IServiceProvider serviceProvider = context.RequestServices;

                var tokenService = serviceProvider.GetService<ITokenService>();

                if (tokenService is null) throw new ArgumentException(nameof(tokenService));

                context.Response.StatusCode = 200;

                context.Response.ContentType = "application/json";

                var token = tokenService.RefreshAccessToken(refreshToken);

                await context.Response.WriteAsync(ObjToJson(token));

                return;
            }
            catch (Exception ex)
            {
                await BadRequest(context, new { error = ex.Message });

                return;
            }
        }

        private static async Task BadRequest(HttpContext context, object msg)
        {
            context.Response.StatusCode = 400;

            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(ObjToJson(msg));

            return;
        }

        private static string ObjToJson<TModel>(TModel model)
        {
            return JsonSerializer.Serialize(model);
        }

        #endregion
    }
}