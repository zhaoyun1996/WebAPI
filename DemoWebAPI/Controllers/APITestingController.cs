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
        [HttpPost("CreateAccount")]
        public async Task<ServiceRespone> CreateAccount(account account)
        {
            return await Insert(account);
        }

        [HttpPut("EditAccount")]
        public async Task<ServiceRespone> EditAccount(account account)
        {
            return await Edit(account);
        }
    }
}
