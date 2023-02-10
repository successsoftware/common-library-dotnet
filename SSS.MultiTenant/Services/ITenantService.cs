﻿using Microsoft.AspNetCore.Http;

namespace SSS.MultiTenant.Services
{
    public interface ITenantService
    {
        IEnumerable<string> GetAllTenantNames();
        Task<string> CreateAsync(string tenant);
        Task RemoveTenantAsync(string id);
        Task<string> GetTenantIdAsync(string tenant);
        string GetTenantName(HttpContext httpContext);
        string GetTenantName(string url);
        Task<string> GetTenantNameAsync(string tenantId);
    }
}