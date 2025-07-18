using System.ComponentModel.DataAnnotations;

namespace ID.Infrastructure.Enums
{
    public enum ApiServiceNames
    {
        [Display(Name = "DalApi", Description = "DBGateApi endpoint ", ResourceType = typeof(string))]
        DalApi
    }
}
