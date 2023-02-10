using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace SSS.AspNetCore.Extensions.Swagger
{
    /// <summary>
    /// Set AdditionalPropertiesAllowed to `true` if AdditionalProperties is NULL. This will prevent `additionalProperties` to be generated to the JSON file.
    /// </summary>
    public class SwaggerAdditionalParametersDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument openApiDoc, DocumentFilterContext context)
        {
            foreach (var schema in context.SchemaRepository.Schemas.Where(schema =>
                schema.Value.AdditionalProperties == null))
            {
                schema.Value.AdditionalPropertiesAllowed = true;
            }
        }
    }
}