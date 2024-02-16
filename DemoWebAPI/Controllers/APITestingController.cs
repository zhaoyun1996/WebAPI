﻿using DemoWebAPI.Base.Controllers;
using DemoWebAPI.Base.Model;
using DemoWebAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APITestingController : BaseController<account>
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

        /// <summary>
        /// Sửa tài khoản
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [HttpPut("EditAccount")]
        public async Task<ServiceRespone> EditAccount(account account)
        {
            return await Edit(account);
        }
    }
}
