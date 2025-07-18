using System.ComponentModel.DataAnnotations;

namespace Intellidesk.Data.Auth
{
    public class ClaimBindModel
    {
        [Required]
        [Display(Name = "Claim Type")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "Claim Value")]
        public string Value { get; set; }
    }
}