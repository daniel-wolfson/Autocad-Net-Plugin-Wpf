using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ID.Infrastructure.Models;

namespace ID.Infrastructure.Extensions
{
    public static partial class ModelStateExtensions
    {
        public static IEnumerable<ApiValidationError> GetModelErrors(this ModelStateDictionary modelState)
        {
            IEnumerable<ApiValidationError> errors = modelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(x => new ApiValidationError(kvp.Key, x.ErrorMessage)))
                .Values
                .SelectMany(x => x);
            return errors;
        }

        public static string[] GetModelStateErrors(this ModelStateDictionary modelState)
        {
            var errors = modelState
                .SelectMany(x => x.Value.Errors)
                .Select(x => x.ErrorMessage)
                .ToArray();
            return errors;
        }
    }
}
