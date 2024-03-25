using DemoWebAPI.Base.Model;
using DemoWebAPI.BL;
using DemoWebAPI.Core.Model;
using DemoWebAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace DemoWebAPI.Base.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        protected readonly CenterConfig _config;
        protected BLAccount _bLAccount = new BLAccount();

        public AuthController(
            CenterConfig config
            )
        {
            _config = config;
        }

        [HttpPost("login")]
        public async Task<ServiceRespone> Login(account account)
        {
            ServiceRespone respone = new ServiceRespone();

            var res = await _bLAccount.Login(account);

            var claimIdentity = new ClaimsIdentity();
            claimIdentity.AddClaim(new Claim("una", account != null ? account.user_name : ""));

            DateTime now = DateTimeUtility.GetNow();
            var tokenExprired = now.AddSeconds(_config.AppSettings.AccessTokenExpiredTime);
            var key = Encoding.ASCII.GetBytes(_config.AppSettings.JwtSecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _config.AppSettings.JwtIssuer,
                Subject = claimIdentity,
                Expires = tokenExprired,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken kTokent = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(kTokent);

            var result = new Dictionary<string, object>()
            {
                {
                    "AccessToken", new
                    {
                        Token = token,
                        TokenExprired = tokenExprired
                    }
                }
            };

            respone.OnSuccess(result);

            return respone;
        }
    }
}
