﻿using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace SSS.AspNetCore.Extensions.Swagger
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                var enumValues = schema.Enum.ToArray();
                var i = 0;
                schema.Enum.Clear();
                foreach (var n in Enum.GetNames(context.Type).ToList())
                {
                    schema.Enum.Add(new OpenApiString(n + $" = {((OpenApiPrimitive<int>)enumValues[i]).Value}"));
                    i++;
                }
            }
        }
    }
}
