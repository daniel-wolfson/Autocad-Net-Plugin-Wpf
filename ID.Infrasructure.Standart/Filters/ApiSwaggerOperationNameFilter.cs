using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace ID.Infrastructure.Filters
{
    public class ApiSwaggerOperationNameFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var authAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<AuthorizeAttribute>();

            //operation.OperationId = null;
            //operation.Summary = "Crpm Summary";
            //operation.Description = "Crpm";

            if (authAttributes.Any())
            {
                //operation.Responses = null;
                operation.Responses.Remove("200");
                operation.Responses.Remove("400");
                //operation.Responses.Add("200", new OpenApiResponse() { Description = "Success" });
                //operation.Responses.Add("400", new OpenApiResponse() { Description = "Badrequest" });
                //operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            }
        }
    }
}
