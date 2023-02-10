using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;

namespace SSS.AspNetCore.Extensions.Handlers
{
    public static class AddErrorCodeHandlerExtensions
    {
        /// <summary>
        /// Allow handle ErrorDetails when ModelState invalid
        /// </summary>
        /// <returns></returns>
        public static IMvcBuilder AddErrorCodeHandler(this IMvcBuilder builder, NamingStrategy namingStrategy = default)
        {
            builder
                .ConfigureApiBehaviorOptions(option =>
                {
                    option.InvalidModelStateResponseFactory = context =>
                    {
                        return TransformModelStateExtensions.TransformModelState(context.ModelState, namingStrategy);
                    };
                });

            return builder;
        }
    }
}
