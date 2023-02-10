using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SSS.MultiTenant.Models;
using SSS.MultiTenant.Services;
using System.Security.Claims;

namespace SSS.MultiTenant
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddTenantDbContext(this IServiceCollection services, IConfiguration configuration, string schema = "oauth")
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<TenantDbContext>(options =>
                options.UseNpgsql(connectionString, configs =>
                {
                    configs.MigrationsHistoryTable("__EFMigrationsHistory", schema);
                }));

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<ITenantService, TenantService>();

            return services;
        }

        public static IServiceCollection ConfigureTenantDb<TContext>(this IServiceCollection services, IConfiguration configuration) where TContext : DbContext
        {
            services.AddScoped(sp =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();

                if (httpContextAccessor.HttpContext == null) return new ServiceTenant();

                if (httpContextAccessor.HttpContext.User == null) return new ServiceTenant();

                var userId = httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var userTenant = httpContextAccessor.HttpContext.User?.FindFirst(CustomClaimTypes.TenantId)?.Value;

                var connectionString = configuration.GetSection("ConnectionStrings").GetValue<string>("DbTemplate").Replace("{tenant}", userTenant);

                var tenant = new ServiceTenant
                {
                    Name = userTenant ?? string.Empty,
                    ConnectionString = connectionString,
                    Subject = userId ?? string.Empty
                };

                return tenant;
            });

            services.AddScoped((serviceProvider) =>
            {
                var tenant = serviceProvider.GetRequiredService<ServiceTenant>();
                var builder = new DbContextOptionsBuilder<TContext>();
                builder.UseNpgsql(tenant.ConnectionString);
                return builder.Options;
            });

            //If Database not exists because a new tenant was registered, create this new one
            services.AddScoped(sp =>
            {
                var options = sp.GetRequiredService<DbContextOptions<TContext>>();
                var context = (TContext)Activator.CreateInstance(typeof(TContext), new object[] { options });
                var exists = ((RelationalDatabaseCreator)context.GetService<IDatabaseCreator>()).Exists();
                if (!exists)
                    context.Database.Migrate();
                return context;
            });

            return services;
        }

        public static IApplicationBuilder MigrateDatabase<TContext>(this IApplicationBuilder app, IConfiguration configuration) where TContext : DbContext
        {
            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

            scope.ServiceProvider.GetRequiredService<TenantDbContext>().Database.Migrate();

            var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();

            var names = tenantService.GetAllTenantNames();

            foreach (var name in names)
            {
                var connectionString = configuration.GetSection("ConnectionStrings")
                    .GetValue<string>("DbTemplate")
                    .Replace("{tenant}", name);
                var options = new DbContextOptionsBuilder<TContext>();
                options.UseNpgsql(connectionString);
                var context = (TContext)Activator.CreateInstance(typeof(TContext), new object[] { options.Options });
                context.Database.Migrate();
                context.Dispose();
            }

            return app;
        }

        public static IApplicationBuilder MigrateDatabase(this IApplicationBuilder app, IConfiguration configuration, Action<string> action)
        {
            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

            scope.ServiceProvider.GetRequiredService<TenantDbContext>().Database.Migrate();

            var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();

            var names = tenantService.GetAllTenantNames();

            foreach (string name in names)
            {
                string connectionString = configuration.GetSection("ConnectionStrings").GetValue<string>("DbTemplate").Replace("{tenant}", name);
                action.Invoke(connectionString);
            }

            return app;
        }
    }
}
