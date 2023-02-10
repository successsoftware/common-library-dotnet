using Microsoft.OpenApi.Any;

namespace SSS.AspNetCore.Extensions.Swagger
{
    internal class XLogoExtention : OpenApiObject, IApiDocExtension
    {
        public XLogoExtention(XLogoOptions options)
        {
            Add("url", new OpenApiString(options.Url));
            Add("altText", new OpenApiString(options.AltText));
            Add("backgroundColor", new OpenApiString(options.BackgroundColor));
            Add("href", new OpenApiString(options.Href));
        }

        public string Name => "x-logo";
    }
}