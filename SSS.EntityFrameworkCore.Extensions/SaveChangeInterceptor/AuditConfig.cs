using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SSS.EntityFrameworkCore.Extensions.SaveChangeInterceptor;
using System;

namespace SSS.EntityFrameworkCore.Extensions
{
    public static class AuditConfig
    {
        public static IServiceCollection AddAuditContext<TContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder)
            where TContext : DbContext
        {
            return services.AddDbContext<TContext>(options =>
            {
                options.AddInterceptors(new AuditingInterceptor());
                optionsBuilder.Invoke(options);
            });
        }
    }
}
