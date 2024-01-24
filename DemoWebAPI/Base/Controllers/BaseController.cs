using DemoWebAPI.Base.BL;
using DemoWebAPI.Base.Model;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace DemoWebAPI.Base.Controllers
{
    public class BaseController<TModel> : ControllerBase where TModel : BaseModel
    {
        protected BLBase<TModel> _bLBase = new BLBase<TModel>();

        [HttpPost]
        public virtual async Task<ServiceRespone> Insert(TModel model)
        {
            return await _bLBase.Insert(model); 
        }

        [HttpGet]
        public virtual async Task<ServiceRespone> GetAll()
        {
            return await _bLBase.GetAll();
        }
    }
}
