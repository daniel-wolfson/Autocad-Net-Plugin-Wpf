using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ID.Infrastructure.Enums
{
    public enum PageRender
    {
        [Display(Name = "resources", Description = "json $page.resources")]
        [DefaultValue("resources")]
        Resources,

        [Display(Name = "data", Description = "json $page.data")]
        [DefaultValue("data")]
        Data,

        [Display(Name = "config", Description = "json $page.config")]
        [DefaultValue("config")]
        Config,

        [Display(Name = "", Description = "json $page.[property]")]
        [DefaultValue("")]
        Properties,


        [Display(Name = "pageModel", Description = "page model name")]
        [DefaultValue("pageModel")]
        Model,

        [Display(Name = "", Description = "json witout var name, for example <var123 = {json}>")]
        [DefaultValue("")]
        None
    }
}
