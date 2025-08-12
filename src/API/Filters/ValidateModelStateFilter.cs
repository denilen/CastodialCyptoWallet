using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CryptoWallet.API.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace CryptoWallet.API.Filters;

/// <summary>
/// Validates the model state and returns a standardized error response if validation fails
/// </summary>
public class ValidateModelStateFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = new Dictionary<string, string[]>();
            
            // Group errors by property name
            foreach (var key in context.ModelState.Keys)
            {
                var state = context.ModelState[key];
                if (state?.Errors != null && state.Errors.Count > 0)
                {
                    var errorMessages = state.Errors
                        .Select(e => e.ErrorMessage)
                        .Where(e => !string.IsNullOrEmpty(e))
                        .ToArray();
                    
                    if (errorMessages.Length > 0)
                    {
                        errors.Add(key, errorMessages);
                    }
                }
            }
            
            // Create a standardized error response
            var response = new ApiResponse<object>
            {
                Success = false,
                Error = "One or more validation errors occurred.",
                ValidationErrors = errors
            };
            
            context.Result = new BadRequestObjectResult(response);
        }
    }
    
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after the action executes
    }
}

/// <summary>
/// Attribute to apply the ValidateModelStateFilter to controllers or actions
/// </summary>
public class ValidateModelStateAttribute : TypeFilterAttribute
{
    public ValidateModelStateAttribute() : base(typeof(ValidateModelStateFilter))
    {
    }
}
