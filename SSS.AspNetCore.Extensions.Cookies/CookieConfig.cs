using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace SSS.AspNetCore.Extensions.Cookies
{
    public static class CookieConfig
    {
        public static AuthenticationBuilder AddCookieAuth(this AuthenticationBuilder services, IHostingEnvironment env, bool includesLaxPolicy = true)
            => services.AddCookieAuth<DefaultCookieEvents>(env, includesLaxPolicy);

        public static AuthenticationBuilder AddCookieAuth(this AuthenticationBuilder services, string scheme, IHostingEnvironment env, bool includesLaxPolicy = true)
            => services.AddCookieAuth<DefaultCookieEvents>(scheme, env, includesLaxPolicy);

        public static AuthenticationBuilder AddCookieAuth<TEvents>(this AuthenticationBuilder services, IHostingEnvironment env, bool includesLaxPolicy = true) where TEvents : DefaultCookieEvents
            => services.AddCookieAuth<TEvents>(CookieAuthenticationDefaults.AuthenticationScheme, env, includesLaxPolicy);

        public static AuthenticationBuilder AddCookieAuth<TEvents>(this AuthenticationBuilder services, string scheme, IHostingEnvironment env, bool includesLaxPolicy = true) where TEvents : DefaultCookieEvents
        {
            services.Services.AddSingleton<TEvents>();

            if (includesLaxPolicy)
                services.Services.AddLaxCookiePolicy(env);

            return services.AddCookie(scheme, op =>
               {
                   op.SlidingExpiration = false;
                   op.EventsType = typeof(TEvents);
               });
        }

        public static IServiceCollection AddLaxCookiePolicy(this IServiceCollection services, IHostingEnvironment env)
           => services.Configure<CookiePolicyOptions>(op =>
           {
               op.MinimumSameSitePolicy = SameSiteMode.Lax;

               op.Secure = env.IsDevelopment()
                   ? CookieSecurePolicy.None
                   : CookieSecurePolicy.Always;

               op.HttpOnly = env.IsDevelopment()
                   ? HttpOnlyPolicy.None
                   : HttpOnlyPolicy.Always;
           });
    }
}