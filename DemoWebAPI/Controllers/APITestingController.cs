using DemoWebAPI.Base.Controllers;
using DemoWebAPI.Base.DL;
using DemoWebAPI.Base.Model;
using DemoWebAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APITestingController : BaseController<account>
    {
        [HttpGet("GetAccounts")]
        public List<account> GetAccounts()
        {
            DLBase dLBase = new DLBase();
            string sql = "select * from account;";

            return new List<account>();
        }

        [HttpPost("CreateAccount")]
        public async Task<ServiceRespone> CreateAccount(account account)
        {
            //string sql = $"INSERT INTO public.account\r\n(account_id, user_name, \"password\")\r\nVALUES({Guid.NewGuid()}, {account.user_name}, {account.password});";
            return await base.Insert(account);
        }
    }
}
