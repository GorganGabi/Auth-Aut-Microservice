using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdentityMicroservice.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IConfiguration configuration, IOptions<AppSettings> appSettings)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            _appSettings = appSettings.Value;
        }

        [HttpPost]
        public async Task<ServiceContract> ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ServiceContract(StatusCodes.Status400BadRequest, null, "Bad Model");
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var passwordVerificationResult = await _signInManager.PasswordSignInAsync(user.Email, model.OldPassword, isPersistent: false, lockoutOnFailure: false);
            if (!passwordVerificationResult.Succeeded)
            {
                return new ServiceContract(StatusCodes.Status422UnprocessableEntity, null, "Password doesn't match");
            }

            var result = await _userManager.ResetPasswordAsync(user, code, model.Password);
            if (result.Succeeded)
            {
                return new ServiceContract(StatusCodes.Status200OK, null, "Password has been reseted");
            }
            return new ServiceContract(StatusCodes.Status400BadRequest, null, "Something went wrong, try later");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation(string userId, string token)
        {
            return Ok("Forgot Password Request Confirmed");
        }

        [HttpPost]
        public async Task<ServiceContract> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ServiceContract(StatusCodes.Status400BadRequest, null, "Bad Model");
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return new ServiceContract(StatusCodes.Status404NotFound, null, "User hasn't been found");
            }
            var rToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var rTokenLink = Url.Action("ForgotPasswordConfirmation", "Account", new
            {
                userId = user.Id,
                token = rToken
            }, protocol: Request.Scheme);

            SMTPClient.SendResetPasswordEmail(model.Email, rTokenLink);
            return new ServiceContract(StatusCodes.Status200OK, null, "Email to reset password has been sent");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return BadRequest();
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest();
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Ok("Succes");
            }
            else
            {
                return NotFound("Email has not been confirmed");
            }
        }

        [HttpPost]
        public async Task<ServiceContract> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ServiceContract(StatusCodes.Status400BadRequest, null, "Bad Model");
            }

            var user = new ApplicationUser { Email = model.Email, UserName = model.Email, Role = model.Role };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {               
                var cToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var cTokenLink = Url.Action("ConfirmEmail", "Account", new
                {
                    userId = user.Id,
                    token = cToken
                }, protocol: Request.Scheme);
                SMTPClient.SendConfirmationEmail(user.Email, cTokenLink);

                var resultModel = new ResultModel(user.Id, "", user.Role);
                return new ServiceContract(StatusCodes.Status200OK, resultModel, "User created succesfully");
            }
            else
            {
                return new ServiceContract(StatusCodes.Status422UnprocessableEntity, null, "User already exists");
            }

        }

        [HttpPost]
        public async Task<ServiceContract> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ServiceContract(StatusCodes.Status400BadRequest, null, "Bad Model");
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var resultModel = new ResultModel(user.Id, "", user.Role);

                if (user.Role != model.Role)
                {
                    return new ServiceContract(StatusCodes.Status422UnprocessableEntity, null, "Role doesn't match");
                }
                else
                {
                    return new ServiceContract(StatusCodes.Status200OK, resultModel, "User logged succesfully");
                }
            }
            else
            {
                return new ServiceContract(StatusCodes.Status422UnprocessableEntity, null, "Couldn't find the user");
            }
        }
    }
}