using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SSS.AspNetCore.Extensions.Swagger
{
    public static class SwaggerConfig
    {
        public static IServiceCollection AddApiDocExtension<TExtension>(this IServiceCollection services)
            where TExtension : class, IApiDocExtension
            => services.AddSingleton<IApiDocExtension, TExtension>();

        public static IServiceCollection AddApiDocExtension(this IServiceCollection services,
            IApiDocExtension extension)
            => services.AddSingleton(extension);

        /// <summary>
        /// helper.AddSecurityDefinition("Bearer", "Authorization");
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static SwaggerSecurityDefinitionHelper AddBearerSecurityDefinition(this SwaggerSecurityDefinitionHelper helper)
        {
            // Bearer token authentication
            var securityDefinition = new OpenApiSecurityScheme()
            {
                Name = "Bearer",
                BearerFormat = "JWT",
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "Specify the authorization token. Example: Bearer {token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
            };

            helper.AddSecurityDefinition("Bearer", securityDefinition);

            // Make sure swagger UI requires a Bearer token specified
            OpenApiSecurityScheme securityScheme = new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference()
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            };

            OpenApiSecurityRequirement securityRequirements = new OpenApiSecurityRequirement()
            {
                {securityScheme, new string[] { }},
            };

            helper.AddSecurityRequirement(securityRequirements);

            return helper;
        }

        /// <summary>
        /// Add SecurityDefinition and SecurityRequirement. Ex: Bearer, Authorization
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="scheme">Ex: Bearer</param>
        /// <param name="headerKey">Ex: Authorization</param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static SwaggerSecurityDefinitionHelper AddSecurityDefinition(this SwaggerSecurityDefinitionHelper helper,
            string scheme,
            string headerKey,
            string description = null,
            ParameterLocation location = ParameterLocation.Header,
            SecuritySchemeType type = SecuritySchemeType.ApiKey)
        {
            helper.AddSecurityDefinition(scheme, new OpenApiSecurityScheme
            {
                In = location,
                Name = headerKey,
                Description = description ?? $"Please enter {scheme} into field",
                Type = type,
                BearerFormat = $"{scheme} YOUR_ACCESS_TOKEN",
            });

            var a = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Scheme = scheme,
                        Reference = new OpenApiReference {Id = scheme, Type = ReferenceType.SecurityScheme}
                    },
                    Array.Empty<string>()
                }
            };
            helper.AddSecurityRequirement(a);

            return helper;
        }

        /// <summary>
        /// AddSwaggerConfig
        /// </summary>
        /// <param name="services"></param>
        /// <param name="info">SwaggerInfo</param>
        /// <param name="securityOptions">The securityOptions. If it is not provided the BEARE token will be added by default.</param>
        /// <param name="xmlFile">addition XML description file.</param>
        /// <param name="genOption">extra Swagger Gen Options</param>
        /// <returns></returns>
        public static IServiceCollection AddSwaggerConfig(this IServiceCollection services, SwaggerInfo info,
            Action<SwaggerSecurityDefinitionHelper> securityOptions = null, string xmlFile = null,
            Action<SwaggerGenOptions> genOption = null)
        {
            if (info != null)
                services.AddSingleton(info);

            services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, SwaggerDefaultOptions>();

            return services.AddSwaggerGen(c =>
            {
                //Ensure XML Document File of project is checked for both Debug and Release mode
                // Set the comments path for the Swagger JSON and UI.
                if (string.IsNullOrEmpty(xmlFile))
                    xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);
                else
                    Trace.TraceWarning(
                        $"the XML {xmlPath} is not found. Ensure the Document File Generation is enabled.");

                c.OperationFilter<SwaggerDefaultOperationFilter>();
                c.DocumentFilter<SwaggerAdditionalParametersDocumentFilter>();
                c.DocumentFilter<SwaggerAddEnumDescriptionsFilter>();
                //c.SchemaFilter<EnumSchemaFilter>();

                genOption?.Invoke(c);

                var security = new SwaggerSecurityDefinitionHelper(c);
                if (securityOptions == null)
                {
                    security.AddBearerSecurityDefinition();
                }
                else securityOptions.Invoke(security);
            });
        }

        public static IServiceCollection AddXLogoExtension(this IServiceCollection services, XLogoOptions options) =>
            services.AddApiDocExtension(new XLogoExtention(options));

        /// <summary>
        /// Add XLogo Extension from configuration. The XLogoOptions will be bind from configuration with section name is `XLogo`
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddXLogoExtension(this IServiceCollection services,
            IConfiguration configuration)
        {
            var options = configuration.Bind<XLogoOptions>(XLogoOptions.Name);

            return services.AddXLogoExtension(options);
        }

        /// <summary>
        /// Use Swagger UI. Defaults route is docs/
        /// </summary>
        /// <param name="app"></param>
        /// <param name="provider"></param>
        /// <param name="route"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSwaggerAndUI(this IApplicationBuilder app, string route = "swagger", Action<SwaggerOptions> setupAction = null)
        {
            var provider = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();

            setupAction ??= c =>
            {
                c.PreSerializeFilters.Add((swagger, httpReq) =>
                {
                    swagger.Servers = new List<OpenApiServer>
                        {new OpenApiServer {Url = $"{httpReq.Scheme}://{httpReq.Host.Value}"}};
                });
            };

            return app.UseSwagger(setupAction)
                .UseSwaggerUI(c =>
                {
                    c.RoutePrefix = route;

                    // build a swagger endpoint for each discovered API version
                    foreach (var description in provider.ApiVersionDescriptions)
                        c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());
                });
        }
    }
}