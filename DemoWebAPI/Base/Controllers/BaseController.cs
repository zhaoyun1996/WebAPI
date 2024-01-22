using DemoWebAPI.Base.BL;
using DemoWebAPI.Base.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebAPI.Base.Controllers
{
    public class BaseController<TModel> : ControllerBase
    {
        [HttpPost]
        public virtual async Task<ServiceRespone> Insert(TModel model)
        {
            BLBase bLBase = new BLBase(model);
            return bLBase.Insert(model); 
        }
    }
}
