using Microsoft.AspNetCore.Mvc;
namespace ID.Api.Extensions
{
    public static class ObjectResultExtensions
    {
        public static bool IsSuccessStatusCode(this ObjectResult objectResult)
        {
            return objectResult.StatusCode != null && objectResult.StatusCode >= 200 && objectResult.StatusCode <= 299;
        }
    }
}
