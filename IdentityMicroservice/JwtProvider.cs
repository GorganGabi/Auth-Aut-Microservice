using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IdentityMicroservice
{
    public class JwtProvider
    {
        private readonly string audience;

        protected readonly IConfiguration configuration;
        private readonly string issuer;
        private readonly SigningCredentials signingCredentials;
        private readonly TimeSpan tokenExpiration;

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        protected JwtProvider(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            this._signInManager = signInManager;
            this.configuration = configuration;
            this._userManager = userManager;

            var jwtConfiguration = configuration.GetSection("Jwt");
            var tokenExpirationConfigValue = jwtConfiguration["TokenExpirationInHours"];

            tokenExpiration = TimeSpan.FromHours(int.Parse(tokenExpirationConfigValue));
            issuer = jwtConfiguration["Issuer"];
            audience = jwtConfiguration["Audience"];

            var privateKey = jwtConfiguration["Key"];
            var symmetricKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(privateKey));
            signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
        }

        public Task CreateToken(HttpContext context)
        {
            string email = context.Request.Form["email"];
            string password = context.Request.Form["password"];

            var user =  _userManager.FindByEmailAsync(email);
            var userLoginResult = _signInManager.CheckPasswordSignInAsync(user.Result, password, false);

            if (!userLoginResult.IsCompleted)
            {
                return BadCredentialsResponse(context.Response);
            }

            return GenerateToken(context.Response, user.Result);
        }

        private Task BadCredentialsResponse(HttpResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            return response.WriteAsync("Bad credentials");
        }

        private Task GenerateToken(HttpResponse response, ApplicationUser user)
        {
            var now = DateTime.Now;

            var claims = GetClaims(user, now);

            var encodedToken = GetEncodedToken(claims, now);

            var jwt = new
            {
                token = encodedToken,
                expiration = tokenExpiration.TotalSeconds
            };

            response.ContentType = "application/json";
            return response.WriteAsync(JsonConvert.SerializeObject(jwt));
        }

        private string GetEncodedToken(IEnumerable<Claim> claims, DateTime generationTime)
        {
            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                generationTime,
                generationTime.Add(tokenExpiration),
                signingCredentials);

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);
            return encodedToken;
        }

        private IEnumerable<Claim> GetClaims(ApplicationUser user, DateTime generationTime)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Iss, issuer),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(generationTime).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Exp,
                    new DateTimeOffset(generationTime).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("id", user.Id.ToString())
            };

            return claims;
        }
    }
}