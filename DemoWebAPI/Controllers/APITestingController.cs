using DemoWebAPI.Base.DL;
using DemoWebAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APITestingController : ControllerBase
    {
        [HttpGet("GetAccounts")]
        public List<account> GetAccounts()
        {
            DLBase databaseService = new DLBase();
            string sql = "select * from account;";

            return databaseService.ExecuteData<account>(sql);
        }

        [HttpPost("CreateAccount")]
        public account CreateAccount(account account)
        {
            DLBase databaseService = new DLBase();
            string sql = $"INSERT INTO public.account\r\n(account_id, user_name, \"password\")\r\nVALUES({Guid.NewGuid()}, {account.user_name}, {account.password});";
            var x = databaseService.ExecuteData<account>(sql);

            return account;
        }
    }
}
