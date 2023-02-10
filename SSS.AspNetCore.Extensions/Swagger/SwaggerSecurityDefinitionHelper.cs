using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SSS.AspNetCore.Extensions.Swagger
{
    public class SwaggerSecurityDefinitionHelper
    {
        private readonly SwaggerGenOptions _options;

        internal SwaggerSecurityDefinitionHelper(SwaggerGenOptions options) => this._options = options;

        internal void AddSecurityDefinition(string name, OpenApiSecurityScheme scheme) => _options.AddSecurityDefinition(name, scheme);

        internal void AddSecurityRequirement(OpenApiSecurityRequirement requirement) => _options.AddSecurityRequirement(requirement);
    }
}