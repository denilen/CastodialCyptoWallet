using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CryptoWallet.API.Filters;

/// <summary>
/// Adds standard response headers to Swagger documentation
/// </summary>
public class AddResponseHeadersFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        // Add standard response headers
        operation.Responses.Add("200", new OpenApiResponse { Description = "Success" });
        operation.Responses.Add("201", new OpenApiResponse { Description = "Created" });
        operation.Responses.Add("204", new OpenApiResponse { Description = "No Content" });
        operation.Responses.Add("400", new OpenApiResponse { Description = "Bad Request" });
        operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });
        operation.Responses.Add("404", new OpenApiResponse { Description = "Not Found" });
        operation.Responses.Add("500", new OpenApiResponse { Description = "Server Error" });

        // Add standard parameters
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Request-ID",
            In = ParameterLocation.Header,
            Description = "Unique request ID for tracing",
            Required = false,
            Schema = new OpenApiSchema { Type = "string" }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-API-Key",
            In = ParameterLocation.Header,
            Description = "API key for authentication",
            Required = true,
            Schema = new OpenApiSchema { Type = "string" }
        });
    }
}
