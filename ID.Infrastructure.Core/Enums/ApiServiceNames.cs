using System.ComponentModel.DataAnnotations;

namespace ID.Infrastructure.Enums
{
    public enum ApiServiceNames
    {
        [Display(Name = "Gateway", Description = "Gateway endpoint ", ResourceType = typeof(string))]
        Gateway,
        [Display(Name = "CalcApi", Description = "CalcApi endpoint ", ResourceType = typeof(string))]
        CalcApi,
        [Display(Name = "ExpertApi", Description = "ExpertApi endpoint ", ResourceType = typeof(string))]
        ExpertApi,
        [Display(Name = "FillerApi", Description = "FillerApi endpoint ", ResourceType = typeof(string))]
        FillerApi,
        [Display(Name = "ReportApi", Description = "ReportApi endpoint ", ResourceType = typeof(string))]
        ReportApi,
        [Display(Name = "EmsApi", Description = "EmsApi endpoint ", ResourceType = typeof(string))]
        EmsApi,
        [Display(Name = "DalApi", Description = "DBGateApi endpoint ", ResourceType = typeof(string))]
        DalApi
    }
}
