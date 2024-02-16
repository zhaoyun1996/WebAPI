using DemoWebAPI.Base.BL;
using DemoWebAPI.Base.Model;
using DemoWebAPI.DL;
using DemoWebAPI.Model;

namespace DemoWebAPI.BL
{
    public class BLAccount : BLBase<account>
    {
        protected DLAccount _dLAccount = new DLAccount();

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<ServiceRespone> Login(account model)
        {
            ServiceRespone res = new ServiceRespone();
            try
            {
                res = await _dLAccount.Login(model);
            }
            catch (Exception)
            {
                throw new Exception();
            }
            return res;
        }
    }
}
