using AspNet.Identity.PostgreSQL;
using ID.Infrastructure;
using Intellidesk.Data.Auth;
using Intellidesk.Data.Services;
using Microsoft.AspNet.Identity;
using Serilog;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Results;
using Unity;

namespace MapIt.WebApi.Controllers
{
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private readonly IAdminService<IdentityUserDetails> _userService;

        private AppUserManager _appUserManager;
        public AppUserManager AppUserManager
        {
            get
            {
                if (_appUserManager == null)
                    _appUserManager = HttpContext.Current.GetOwinContext().Get<AppUserManager>("AppUserManager");
                return _appUserManager;
            }
        }

        public AccountController() //IAdminService userService
        {
            Plugin.Create(HostingEnvironment.MapPath(@"~/App_Data")).Register(new UnityContainer());
            _userService = Plugin.GetService<IAdminService<IdentityUserDetails>>();
        }

        // POST api/Account/Register
        [Route("Register")]
        [HttpPost]
        public async Task<IHttpActionResult> Register([FromBody] UserData userModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new IdentityUserDetails() { UserName = userModel.Email, Email = userModel.Email };
            var result = await AppUserManager.CreateAsync(user, userModel.Password);

            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                Log.Logger.Error(errorResult.ToString());
                return errorResult;
            }

            return Ok();
        }



        // GET api/values
        [Route("accesstoken")]
        [HttpPost]
        public async Task<IHttpActionResult> AccessToken(UserAccessBindModel userParams)
        {
            if (!ModelState.IsValid)
                return BadRequest("User data is incorrect");

            var user = await _userService.AuthenticateWithUserManager(userParams.UserName, userParams.Password);
            if (user == null)
            {
                string message = "User not found";
                //_logger.Info($"{nameof(Login)} action: {message}");
                return Ok(new { errorMessage = message });
            }
            else
            {
                //_logger.Info($"{nameof(Login)} authenticated user: idNumber={user.IdNumber}; token={user.Token}");
            }

            return Ok(user);
        }

        [Route("CheckUserName")]
        public JsonResult<bool> CheckUserName(string email, string password)
        {
            var result = _userService.FindAsync(email, password) == null;
            return Json(result);
        }

        [Route("Validate1")]
        [HttpGet]
        public object Validate(string token, string username)
        {
            //int UserId = new UserRepository().GetUser(username);
            //if (UserId == 0) return new ResponseVM { Status = "Invalid", Message = "Invalid User." };
            //string tokenUsername = TokenManager.ValidateToken(token);
            //if (username.Equals(tokenUsername))
            //{
            //    return new ResponseVM
            //    {
            //        Status = "Success",
            //        Message = "OK",
            //    };
            // }
            return new { Status = "Invalid", Message = "Invalid Token." };
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}
