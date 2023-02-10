using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using SSS.AspNetCore.Extensions.Handlers;

namespace SSS.AspNetCore.Extensions
{
    public static class FluentValidationConfig
    {
        public static IServiceCollection AddValidation(this IServiceCollection services)
        {

            return services.AddSingleton<IValidatorHandler, ValidatorHandler>();
        }

        public static IServiceCollection AddValidation<TStartup>(this IServiceCollection services)
        {
            return services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
        }
    }
}
