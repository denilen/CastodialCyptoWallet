using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Microsoft.OpenApi.Any;
using System.ComponentModel;
using System.Linq;

namespace CryptoWallet.API.Filters;

/// <summary>
/// Adds default values and descriptions to Swagger documentation
/// </summary>
public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;
        
        // Set the operation ID if not already set
        if (operation.OperationId == null)
        {
            operation.OperationId = apiDescription.ActionDescriptor.RouteValues["action"];
        }
        
        // Add default values and descriptions for parameters
        foreach (var parameter in operation.Parameters)
        {
            var description = apiDescription.ParameterDescriptions
                .First(p => p.Name == parameter.Name);
                
            // Set parameter description if not already set
            if (parameter.Description == null)
            {
                parameter.Description = description.ModelMetadata?.Description;
            }
            
            // Set default value if available
            var defaultAttribute = description.ParameterDescriptor?.ParameterType
                .GetProperty(description.Name)
                ?.GetCustomAttribute<DefaultValueAttribute>();
                
            if (defaultAttribute != null)
            {
                parameter.Schema.Default = new OpenApiString(defaultAttribute.Value?.ToString());
            }
            
            // Set required flag
            if (parameter.In.HasValue && parameter.In.Value == ParameterLocation.Path)
            {
                parameter.Required = true;
            }
            
            // Add enum values to the description
            if (parameter.Schema.Enum != null && parameter.Schema.Enum.Count > 0)
            {
                var enumValues = string.Join(", ", parameter.Schema.Enum.Select(e => e.ToString()));
                parameter.Description = $"{parameter.Description ?? ""} (Values: {enumValues})";
            }
        }
        
        // Add default response if none is set
        if (!operation.Responses.Any())
        {
            operation.Responses.Add("200", new OpenApiResponse { Description = "Success" });
        }
    }
}
