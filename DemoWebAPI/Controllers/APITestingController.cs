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
        public async Task<ServiceRespone> GetAccounts()
        {
            return await GetAll();
        }

        [HttpPost("CreateAccount")]
        public async Task<ServiceRespone> CreateAccount(account account)
        {
            return await Insert(account);
        }
    }
}
