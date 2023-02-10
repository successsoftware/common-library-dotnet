using AutoMapper;
using System.Reflection;

namespace Demo.NetKit.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
        }

        private void ApplyMappingsFromAssembly(Assembly assembly)
        {
            var types = assembly.GetExportedTypes()
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && (i.GetGenericTypeDefinition().Name.Equals("IMapFrom`1") || i.GetGenericTypeDefinition().Name.Equals("IMapTo`1"))))
                .ToList();

            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);

                var methodInfo = type.GetMethod("Mapping")
                    ?? (type.GetInterface("IMapFrom`1").GetMethod("Mapping") ?? type.GetInterface("IMapTo`1").GetMethod("Mapping"));

                methodInfo?.Invoke(instance, new object[] { this });
            }
        }
    }
}
