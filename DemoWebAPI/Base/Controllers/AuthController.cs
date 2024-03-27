using DemoWebAPI.Base.Model;
using DemoWebAPI.BL;
using DemoWebAPI.Core.Model;
using DemoWebAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
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
        public async Task<ServiceResponse> Login(account account)
        {
            ServiceResponse respone = await _bLAccount.Login(account);

            if(respone.Success)
            {
                respone = await _bLAccount.SetCacheRedis(respone, account, _config);
            }

            return respone;
        }
    }
}
