using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SSS.AspNetCore.Extensions
{
    public static class DataProtectionConfig
    {
        public static IServiceCollection AddDataProtection(this IServiceCollection services, Action<IDataProtectionBuilder> builder)
        {
            var p = services.AddDataProtection()
                   .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
                   {
                       EncryptionAlgorithm = EncryptionAlgorithm.AES_256_GCM,
                       ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                   });
            builder?.Invoke(p);
            return services;
        }
    }

    public sealed class AzKeyBlobInfo
    {
        public string BlobName { get; set; }
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
    }
}