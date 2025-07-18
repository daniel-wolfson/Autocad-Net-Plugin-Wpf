using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace ID.Infrastructure.Filters
{
    //public class ApiSwaggerOperationAttribute : SwaggerOperationAttribute, IOperationFilter
    //{
    //    public ApiSwaggerOperationAttribute(string summary = null, string description = null) : base(summary, description)
    //    { 
    //    }

    //    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    //    {
    //        //throw new System.NotImplementedException();
    //    }
    //}

    //public class ApiSwaggerResponseAttribute : SwaggerResponseAttribute, IFilterMetadata
    //{
    //    public ApiSwaggerResponseAttribute(int statusCode, string description = null) : base(statusCode, description)
    //    {
    //    }
    //}

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    class SwaggerDefaultValueAttribute : Attribute
    {
        public SwaggerDefaultValueAttribute(string parameterName, object value)
        {
            this.ParameterName = parameterName;
            this.Value = value;
        }

        public string ParameterName { get; private set; }

        public object Value { get; set; }
    }
}