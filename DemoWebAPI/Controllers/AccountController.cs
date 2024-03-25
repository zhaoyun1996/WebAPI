using DemoWebAPI.Base.Controllers;
using DemoWebAPI.Base.Model;
using DemoWebAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseController<account>
    {
        /// <summary>
        /// Thêm tài khoản
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpPost("CreateAccount")]
        public async Task<ServiceRespone> CreateAccount(account account)
        {
            return await Insert(account);
        }
    }
}
