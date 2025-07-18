using AutoMapper;
using ID.Api.Interfaces;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using ID.WebApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;

namespace ID.Api.Controllers
{
    //[Authorize]
    [Route("/api/DalApi/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public AccountController(IUserService userService, ILogger logger, IMapper mapper)
        {
            _userService = userService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("GetUsers")]
        public IActionResult GetUsers()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpGet("GetById")]
        public IActionResult GetById(Guid id)
        {
            IUserDetails user = _userService.GetById(id);
            return Ok(user);
        }

        [HttpPut("Update")]
        public IActionResult Update(Guid id, [FromBody] IUserDetails userDetails)
        {
            userDetails.Id = id;

            try
            {
                _userService.Update(userDetails, userDetails.PasswordHash);
                return Ok();
            }
            catch (ApiException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("Delete")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("GetToken")]
        public async Task<IActionResult> GetToken([FromForm] LoginModel loginModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { errorMessage = "User or password is incorrect" });

            var token = await _userService.Authenticate(loginModel.Email, loginModel.PasswordHash);
            if (string.IsNullOrEmpty(token))
            {
                string message = "User not authenticated";
                _logger.Information($"{nameof(GetToken)} action: {message}");
                return NotFound(new { errorMessage = message });
            }
            else
            {
                await _userService.WriteLocalTokenAsync(token);
            }

            return new JsonResult(token);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromForm] Models.AppUserModel userModel)
        {
            var user = _mapper.Map<IUserDetails>(userModel);
            try
            {
                _userService.Create(user, userModel.PasswordHash);
                return Ok();
            }
            catch (ApiException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
