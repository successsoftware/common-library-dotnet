using Microsoft.OpenApi.Interfaces;

namespace SSS.AspNetCore.Extensions.Swagger
{
    public interface IApiDocExtension : IOpenApiExtension
    {
        string Name { get; }
    }
}