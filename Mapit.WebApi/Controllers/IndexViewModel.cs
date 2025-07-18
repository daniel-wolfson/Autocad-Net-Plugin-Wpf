using System.Collections.Generic;
using Microsoft.AspNet.Identity;

namespace MapIt.WebApi.Controllers
{
    internal class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public bool BrowserRemembered { get; set; }
    }
}