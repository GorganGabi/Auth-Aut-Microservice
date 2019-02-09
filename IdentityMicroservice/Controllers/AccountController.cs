using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }


        public IActionResult ForgotPasswordConfirmation(string userId, string token)
        {
            return Ok("Forgot Password Request Confirmed");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return NotFound();
            }
            var rToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            //var rTokenLink = Url.ResetPasswordCallbackLink(user.Id, rToken, Request.Scheme);
            //var rTokenLink = Url.Link("Default", new
            //{
            //    Controller = "Account",
            //    Action = "ForgotPasswordConfirmation",
            //    userId = user.Id,
            //    token = rToken
            //});
            var rTokenLink = Url.Action("ForgotPasswordConfirmation", "Account", new
            {
                userId = user.Id,
                token = rToken
            }, protocol: Request.Scheme);

            SMTPClient.SendResetPasswordEmail(model.Email, rTokenLink);
            return Ok();
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
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(model);
            }

            var user = new ApplicationUser { Email = model.Email, UserName = model.Email };
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

                return Ok(GetToken(user));
            }
            else
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var resultModel = new ResultModel(user.Id, GetToken(user));
                return Ok(resultModel);
            }
            else
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity);
            }
        }

            private string GetToken(IdentityUser user)
        {
            var utcNow = DateTime.UtcNow;

            var claims = new Claim[]
            {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                        new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, utcNow.ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._configuration.GetValue<String>("Tokens:Key")));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                signingCredentials: signingCredentials,
                claims: claims,
                notBefore: utcNow,
                expires: utcNow.AddSeconds(this._configuration.GetValue<int>("Tokens:Lifetime"))
                );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}