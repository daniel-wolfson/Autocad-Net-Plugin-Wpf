using ID.Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ID.Infrastructure.Formatters
{

    public class FromMultiFormBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Do not use this provider for binding simple values
            if (!context.Metadata.IsComplexType) return null;

            // Do not use this provider if the binding target is not a property
            var propName = context.Metadata.PropertyName;
            var propInfo = context.Metadata.ContainerType?.GetProperty(propName);
            if (propName == null || propInfo == null) return null;

            // Do not use this provider if the target property type implements IFormFile
            //if (propInfo.PropertyType.IsAssignableFrom(typeof(IFormFile))) return null;

            // Do not use this provider if this property does not have the FromForm attribute
            //if (!propInfo.GetCustomAttributes(typeof(FromForm), false).Any()) return null;
            //if (context.Metadata.ModelType != typeof(MultipartFormData2))
            //{
            //    return null;
            //}

            // All criteria met; use the FormDataJsonBinder
            return new FromMultiFormBinder();
        }
    }

    public class FromMultiFormBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

            // Fetch the value of the argument by name and set it to the model state
            string fieldName = bindingContext.FieldName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(fieldName);
            if (valueProviderResult == ValueProviderResult.None) return Task.CompletedTask;
            else bindingContext.ModelState.SetModelValue(fieldName, valueProviderResult);

            // Do nothing if the value is null or empty
            string value = valueProviderResult.FirstValue;
            if (string.IsNullOrEmpty(value)) return Task.CompletedTask;

            try
            {
                // Deserialize the provided value and set the binding result
                bindingContext.HttpContext.Request.Form["fieldName"].ToString();
                object result = Util.ConvertByteArrayToObject(Encoding.UTF8.GetBytes(value)); //bindingContext.ModelType
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            catch (Exception)
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }
}
