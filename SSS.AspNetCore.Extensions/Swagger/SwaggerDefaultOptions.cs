using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace SSS.AspNetCore.Extensions.Swagger
{
    public class SwaggerDefaultOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IEnumerable<IApiDocExtension> _extensions;
        private readonly SwaggerInfo _options;
        private readonly IApiVersionDescriptionProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerDefaultOptions"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger documents.</param>
        /// <param name="options"></param>
        /// <param name="extensions"></param>
        public SwaggerDefaultOptions(IApiVersionDescriptionProvider provider, SwaggerInfo options, IEnumerable<IApiDocExtension> extensions)
        {
            this._provider = provider;
            this._options = options;
            this._extensions = extensions;
        }

        /// <inheritdoc />
        public void Configure(SwaggerGenOptions options)
        {
            // add a swagger document for each discovered API version
            // note: you might choose to skip or document deprecated API versions differently
            foreach (var description in _provider.ApiVersionDescriptions)
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }

        private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo()
            {
                Title = _options.Title ?? "Swagger API",
                Version = description.ApiVersion.ToString(),
                Description = _options.Description ?? _options.Title,
                Contact = string.IsNullOrEmpty(_options.Contact) ? null : new OpenApiContact() { Name = "Support", Email = _options.Contact },
                License = string.IsNullOrEmpty(_options.License) ? null : new OpenApiLicense() { Name = "License", Url = new Uri(_options.License) },
                TermsOfService = string.IsNullOrEmpty(_options.TermsOfService) ? null : new Uri(_options.TermsOfService),
                Extensions = GetDictionary()
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }

        private IDictionary<string, IOpenApiExtension> GetDictionary()
        {
            var dic = new Dictionary<string, IOpenApiExtension>();
            if (_extensions == null) return dic;

            foreach (var ex in _extensions)
            {
                dic.Add(ex.Name, ex);
            }
            return dic;
        }
    }
}