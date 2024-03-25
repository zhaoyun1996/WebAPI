using DemoWebAPI.Core.Model;
using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.Core.Cache
{
    public class SessionHelper
    {
        public static async Task<bool> CheckLoginSession(string sessionId)
        {
            var value = "";
            if(!string.IsNullOrEmpty(value))
            {
                return true;
            }

            return false;
        }
    }
}
