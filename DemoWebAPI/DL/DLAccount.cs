using DemoWebAPI.Base.DL;
using DemoWebAPI.Base.Model;
using DemoWebAPI.Model;
using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.DL
{
    public class DLAccount : DLBase
    {
        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ServiceRespone> Login(account model)
        {
            ServiceRespone res = new ServiceRespone();
            var sql = $"select count(*) from account where user_name = '{model.user_name}' limit 1";
            List<int> userNameValids = QueryCommandTextOld<int>(DatabaseType.Business, DatabaseSide.ReadSide, sql);
            if (userNameValids != null && userNameValids.Count > 0 && userNameValids[0] > 0)
            {
                sql = $"select * from account where user_name = '{model.user_name}' and password = '{model.password}' limit 1";
                List<account> data = QueryCommandTextOld<account>(DatabaseType.Business, DatabaseSide.ReadSide, sql);

                if (userNameValids != null && userNameValids.Count > 0 && userNameValids[0] > 0)
                {
                    res.OnSuccess(model);
                }
                else
                {
                    res.OnError(ServiceResponseCode.InvalidData, 0, model, null, "Password invalid");
                }
            }
            else
            {
                res.OnError(ServiceResponseCode.NotFound, 0, model, null, "Account not found");
            }

            return res;
        }
    }
}
