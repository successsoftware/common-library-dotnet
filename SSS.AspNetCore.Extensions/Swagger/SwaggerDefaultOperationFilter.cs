using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Diagnostics;
using System.Linq;

namespace SSS.AspNetCore.Extensions.Swagger
{
    /// <summary>
    /// Default Operation Filter.
    /// Fixed issues:
    /// - https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
    /// - https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
    /// Auto generate Operation Id if not provided.
    /// </summary>
    public class SwaggerDefaultOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;
            operation.Deprecated |= apiDescription.IsDeprecated();

            //Generate Operation ID if Empty
            if (string.IsNullOrEmpty(operation.OperationId))
            {
                var info = (ControllerActionDescriptor)context.ApiDescription.ActionDescriptor;
                var controllerName = info.ControllerName;
                var actionName = info.ActionName;
                var parameters = operation.Parameters
                    .Where(p => !p.Name.Equals("api-version", StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Name).ToArray();

                operation.OperationId =
                    $"{context.ApiDescription.GroupName}_{controllerName}_{actionName}:{(parameters.Length > 0 ? string.Join("_", parameters) : "")}";

                Trace.TraceInformation(
                    $"The new Operation Id had been assigned: {operation.OperationId} for {context.MethodInfo.Name}");
            }

            if (operation.Parameters == null)
            {
                return;
            }

            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

                parameter.Description ??= description.ModelMetadata?.Description;

                if (parameter.Schema.Default == null && description.DefaultValue != null)
                {
                    parameter.Schema.Default = new OpenApiString(description.DefaultValue.ToString());
                }

                parameter.Required |= description.IsRequired;
            }
        }
    }
}