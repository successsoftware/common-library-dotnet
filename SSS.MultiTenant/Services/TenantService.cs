using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SSS.MultiTenant.Models;
using System.Data.Common;

namespace SSS.MultiTenant.Services
{
    public class TenantService : ITenantService
    {
        private readonly TenantDbContext _context;
        private readonly ILogger<TenantService> _logger;

        public TenantService(
            TenantDbContext context,
            ILogger<TenantService> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GetTenantIdAsync(string tenantName)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Name.Replace(" ", "").ToLower().Equals(tenantName.ToLower()));
            return tenant?.Id ?? string.Empty;
        }

        public async Task RemoveTenantAsync(string id)
        {
            var currentTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id).ConfigureAwait(false);
            if (currentTenant != null)
            {
                try
                {
                    _context.Tenants.Remove(currentTenant);
                    await _context.SaveChangesAsync();
                }
                catch (DbException e)
                {
                    _logger.LogError("Failed to remove tenant with the Id: {id}, Exception {e}", id, e);
                }
            }
        }

        public string GetTenantName(HttpContext httpContext)
        {
            if (!httpContext.Request.Host.HasValue)
                return string.Empty;

            return InternalGetTenantName(httpContext.Request.Host.Host);
        }

        public string GetTenantName(string url)
        {
            return InternalGetTenantName(url);
        }

        public async Task<string> GetTenantNameAsync(string tenantId)
        {
            if (string.IsNullOrEmpty(tenantId))
                return string.Empty;

            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            return tenant?.Name ?? string.Empty;
        }

        public IEnumerable<string> GetAllTenantNames()
        {
            var names = _context.Tenants.Select(t => t.Name).ToArray();
            return names;
        }

        public async Task<string> CreateAsync(string tenant)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var entity = await _context.Tenants.AddAsync(new Tenant()
                {
                    Name = tenant
                });

                await _context.SaveChangesAsync();

                transaction.Commit();

                var id = entity.Entity.Id;

                try
                {
                    return id;
                }
                catch (DbException e)
                {
                    _logger.LogError("Failed to add tenant with the Id: {id}, Exception {e}", id, e);
                }
            }

            return string.Empty;
        }

        private string InternalGetTenantName(string url)
        {
            url = url.Replace("https://", "");
            url = url.Replace("http://", "");

            var splitHost = url.Split('.');

            return splitHost.Length < 2 ? string.Empty : splitHost[0];
        }
    }
}
