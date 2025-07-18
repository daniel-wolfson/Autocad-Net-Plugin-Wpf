using System.ComponentModel.DataAnnotations;

namespace ID.Infrastructure.Enums
{
    public enum LanguagesTypes
    {
        [Display(Name = "en-en", Description = "English")]
        En = 0,
        [Display(Name = "he-il", Description = "Hebrew")]
        He = 1,
        [Display(Name = "ru-ru", Description = "Russian")]
        Ru = 2,
        [Display(Name = "ar-ar", Description = "Arabic")]
        Ar = 3,
    }
}
