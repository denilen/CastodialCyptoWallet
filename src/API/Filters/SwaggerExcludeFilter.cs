using System;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Serialization;

namespace CryptoWallet.API.Filters;

/// <summary>
/// Excludes properties marked with [JsonIgnore] or [SwaggerExclude] from Swagger documentation
/// </summary>
public class SwaggerExcludeFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties == null || context.Type == null)
            return;

        // Get all properties that should be excluded
        var excludedProperties = context.Type.GetProperties()
            .Where(t => 
                t.GetCustomAttribute<JsonIgnoreAttribute>() != null ||
                t.GetCustomAttribute<SwaggerExcludeAttribute>() != null);

        foreach (var excludedProperty in excludedProperties)
        {
            var propertyToRemove = schema.Properties.Keys
                .SingleOrDefault(x => 
                    string.Equals(x, excludedProperty.Name, StringComparison.OrdinalIgnoreCase));
                
            if (propertyToRemove != null)
            {
                schema.Properties.Remove(propertyToRemove);
            }
        }
    }
}

/// <summary>
/// Attribute to mark properties that should be excluded from Swagger documentation
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SwaggerExcludeAttribute : Attribute
{
}
