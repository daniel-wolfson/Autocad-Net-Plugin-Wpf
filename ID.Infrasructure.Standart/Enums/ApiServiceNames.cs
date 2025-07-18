using System.ComponentModel.DataAnnotations;

namespace ID.Infrastructure.Enums
{
    public enum ApiServiceNames
    {
        [Display(Name = "DalApi", Description = "Gateway endpoint ", ResourceType = typeof(string))]
        DalApi
    }
}
