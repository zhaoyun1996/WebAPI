using Azure.Core;
using DemoWebAPI.Base.DL;
using DemoWebAPI.Core.Model;
using Newtonsoft.Json;
using StackExchange.Redis;
using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.Core.Cache
{
    public class SessionHelper
    {
        

        public static async Task<bool> CheckLoginSession(string sessionId)
        {
            // Lưu thông tin đăng nhập vào cache
            

            

            var value = "";
            if(!string.IsNullOrEmpty(value))
            {
                return true;
            }

            return false;
        }
    }
}
