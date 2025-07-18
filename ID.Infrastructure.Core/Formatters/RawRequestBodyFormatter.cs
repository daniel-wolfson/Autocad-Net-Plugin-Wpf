using ID.Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ID.Infrastructure.Formatters
{
    /// <summary>
    /// Formatter that allows content of type text/plain and application/octet stream
    /// or no content type to be parsed to raw data. Allows for a single input parameter
    /// in the form of:
    /// 
    /// public string RawString([FromBody] string data)
    /// public byte[] RawData([FromBody] byte[] data)
    /// </summary>
    public class RawRequestBodyFormatter : InputFormatter
    {
        public RawRequestBodyFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(MediaTypeNames.Application.Octet));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(CustomMediaTypeNames.XWwwFormUrlencoded));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(CustomMediaTypeNames.FormData));
        }


        /// <summary>
        /// Allow text/plain, application/octet-stream and no content type to
        /// be processed
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Boolean CanRead(InputFormatterContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var contentType = context.HttpContext.Request.ContentType;
            if (string.IsNullOrEmpty(contentType)
                || contentType == MediaTypeNames.Text.Plain
                || contentType.StartsWith(CustomMediaTypeNames.FormData)
                || contentType == MediaTypeNames.Application.Octet)
                return true;

            return false;
        }

        /// <summary>
        /// Handle text/plain or no content type for string results
        /// Handle application/octet-stream for byte[] results
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            var contentType = context.HttpContext.Request.ContentType;

            if (string.IsNullOrEmpty(contentType) || contentType == MediaTypeNames.Text.Plain)
            {
                using (var reader = new StreamReader(request.Body))
                {
                    var content = await reader.ReadToEndAsync();
                    return await InputFormatterResult.SuccessAsync(content);
                }
            }

            if (contentType == MediaTypeNames.Application.Octet)
            {
                using (var ms = new MemoryStream(2048))
                {
                    await request.Body.CopyToAsync(ms);
                    var content = ms.ToArray();
                    return await InputFormatterResult.SuccessAsync(content);
                }
            }

            if (context.HttpContext.Request.HasFormContentType)
            {
                Dictionary<string, object> formResult = new Dictionary<string, object>();
                var form = context.HttpContext.Request.Form;

                foreach (var formFile in form.Files)
                {
                    byte[] data;
                    using (var br = new BinaryReader(formFile.OpenReadStream()))
                        data = br.ReadBytes((int)formFile.OpenReadStream().Length);

                    object obj;
                    if (context.HttpContext.Request.Headers.ContainsKey("Compress"))
                        obj = Util.ConvertByteArrayToObject(Util.Decompress((Util.Decompress(data))));
                    else
                        obj = Util.ConvertByteArrayToObject(data);

                    formResult.Add(formFile.Name, obj);
                }

                var result = await InputFormatterResult.SuccessAsync(formResult);
                return result;
            }

            return await InputFormatterResult.FailureAsync();
        }

        private static string GetBoundary(string contentType)
        {
            var elements = contentType.Split(' ');
            var element = elements.Where(entry => entry.StartsWith("boundary=")).First();
            var boundary = element.Substring("boundary=".Length);
            // Remove quotes
            if (boundary.Length >= 2 && boundary[0] == '"' &&
                boundary[boundary.Length - 1] == '"')
            {
                boundary = boundary.Substring(1, boundary.Length - 2);
            }
            return boundary;
        }

        private static object GetTuple<T>(params T[] values)
        {
            Type genericType = Type.GetType("System.Tuple`" + values.Length);
            Type[] typeArgs = values.Select(_ => typeof(T)).ToArray();
            Type specificType = genericType.MakeGenericType(typeArgs);
            object[] constructorArguments = values.Cast<object>().ToArray();
            return Activator.CreateInstance(specificType, constructorArguments);
        }
    }
}
