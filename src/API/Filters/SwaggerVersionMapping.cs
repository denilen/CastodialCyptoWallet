using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace CryptoWallet.API.Filters;

/// <summary>
/// Adds version information to the Swagger UI
/// </summary>
public class SwaggerVersionMapping : IDocumentFilter
{
    private readonly IApiVersionDescriptionProvider _provider;

    public SwaggerVersionMapping(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Add version to the document
        swaggerDoc.Info.Version = _provider.ApiVersionDescriptions
            .FirstOrDefault(x => x.GroupName == context.DocumentName)?.ApiVersion.ToString() ?? "v1";

        // Add contact information
        swaggerDoc.Info.Contact = new OpenApiContact
        {
            Name = "CryptoWallet Support",
            Email = "support@cryptowallet.com",
            Url = new Uri("https://support.cryptowallet.com")
        };

        // Add license information
        swaggerDoc.Info.License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        };

        // Add terms of service
        swaggerDoc.Info.TermsOfService = new Uri("https://cryptowallet.com/terms");

        // Add server information
        swaggerDoc.Servers = new List<OpenApiServer>
        {
            new OpenApiServer { Url = "/api/v1", Description = "Version 1 API" },
            new OpenApiServer { Url = "/", Description = "Current Version API" }
        };

        // Add security definitions
        swaggerDoc.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n" +
                          "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                          "Example: 'Bearer 12345abcdef'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });

        // Add global security requirements
        swaggerDoc.SecurityRequirements = new List<OpenApiSecurityRequirement>
        {
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            }
        };

        // Add API version to the path
        var paths = new OpenApiPaths();
        foreach (var path in swaggerDoc.Paths)
        {
            paths.Add(path.Key.Replace("v{version}", swaggerDoc.Info.Version), path.Value);
        }

        swaggerDoc.Paths = paths;
    }
}
