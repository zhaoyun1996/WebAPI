using DemoWebAPI.Base.BL;
using DemoWebAPI.Base.Model;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebAPI.Base.Controllers
{
    public class BaseController<TModel> : ControllerBase where TModel : BaseModel
    {
        protected BLBase<TModel> _bLBase = new BLBase<TModel>();

        /// <summary>
        /// Thêm bản ghi
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<ServiceRespone> Insert(TModel model)
        {
            return await _bLBase.Insert(model); 
        }

        /// <summary>
        /// Lấy tất cả bản ghi
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="customFilter"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual async Task<ServiceRespone> GetAll(string filter = "", string sort = "", string customFilter = "", string columns = "")
        {
            return await _bLBase.GetAll(filter, sort, customFilter, columns);
        }

        /// <summary>
        /// Xóa bản ghi
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpDelete]
        public virtual async Task<ServiceRespone> Delete(TModel model)
        {
            return await _bLBase.Delete(model);
        }

        /// <summary>
        /// Sửa bản ghi
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        public virtual async Task<ServiceRespone> Edit(TModel model)
        {
            return await _bLBase.Edit(model);
        }
    }
}
