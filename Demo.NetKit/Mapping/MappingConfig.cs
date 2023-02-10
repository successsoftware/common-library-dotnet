using System.Reflection;

namespace Demo.NetKit.Mapping
{
    public static class MappingConfig
    {
        public static IServiceCollection AddMapping(this IServiceCollection services)
        {
            return services.AddAutoMapper(Assembly.GetExecutingAssembly());
        }
    }
}
