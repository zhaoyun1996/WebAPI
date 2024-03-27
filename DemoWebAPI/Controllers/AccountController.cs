using DemoWebAPI.Base.Controllers;
using DemoWebAPI.Base.Model;
using DemoWebAPI.BL;
using DemoWebAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseController<account>
    {
        protected BLAccount _bLAccount = new BLAccount();

        /// <summary>
        /// Thêm tài khoản
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpPost("CreateAccount")]
        public async Task<ServiceResponse> CreateAccount(account account)
        {
            return await Insert(account);
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpPost("Login")]
        public async Task<ServiceResponse> Login(account account)
        {
            return await _bLAccount.Login(account);
        }
    }
}
