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
        public async Task<ServiceResponse> Login(account model)
        {
            ServiceResponse res = new ServiceResponse();

            var sql = $"select * from account where user_name = '{model.user_name}' limit 1";
            List<account> accounts = QueryCommandTextOld<account>(DatabaseType.Business, DatabaseSide.ReadSide, sql);

            if (accounts != null && accounts.Count > 0)
            {
                if (accounts[0].password == model.password)
                {
                    model.account_id = accounts[0].account_id;
                    res.OnSuccess(model);
                }
                else
                {
                    res.OnError(ServiceResponseCode.InvalidData, 0, null, null, "Mật khẩu không chính xác!");
                }
            }
            else
            {
                res.OnError(ServiceResponseCode.NotFound, 0, null, null, "Tài khoản không tồn tại!");
            }

            return res;
        }
    }
}
