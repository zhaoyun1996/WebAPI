using DemoWebAPI.Base.DL;
using DemoWebAPI.Base.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.Base.BL
{
    public partial class BLBase<TModel> where TModel : BaseModel
    {
        protected DLBase _dLBase = new DLBase();

        /// <summary>
        /// Lấy tất cả bản ghi
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="customFilter"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public virtual async Task<ServiceResponse> GetAll(string filter, string sort, string customFilter, string columns)
        {
            ServiceResponse res = new ServiceResponse();
            res.Data = _dLBase.GetAll<TModel>(DatabaseType.Business, filter, sort, customFilter, columns);
            return res;
        }

        /// <summary>
        /// Thêm mới bản ghi
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<ServiceResponse> Insert(TModel model)
        {
            ServiceResponse res = new ServiceResponse();

            IDbConnection cnn = null;
            IDbTransaction transaction = null;

            try
            {
                if (model == null)
                {
                    res.OnError(ServiceResponseCode.InvalidData);
                    return res;
                }

                cnn = _dLBase.GetConnection();
                _dLBase.OpenConnection(cnn);
                transaction = cnn.BeginTransaction(IsolationLevel.ReadUncommitted);

                model.state = ModelState.Insert;

                bool checkDuplicate = _dLBase.CheckDuplicate(model);
                if(checkDuplicate)
                {
                    res.OnError(ServiceResponseCode.Duplicate);
                    return res;
                }

                model.SetAutoPrimaryKey();

                model.created_by = "admin";
                model.modified_by = "admin";
                model.created_date = DateTimeUtility.GetNow();
                model.modified_date = DateTimeUtility.GetNow();

                Dictionary<object, Exception> result = _dLBase.DoSaveBatchData(cnn, transaction, new List<TModel> { model }, ModelState.Insert);

                if (result != null && result.Count > 0)
                {
                    res.OnError(ServiceResponseCode.Error);
                }
                else
                {
                    res.Data = model;
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                throw new Exception();
            }
            finally
            {
                _dLBase.CloseConnection(cnn);
            }

            return res;
        }

        /// <summary>
        /// Xóa bản ghi
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<ServiceResponse> Delete(TModel model)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                _dLBase.Delete(model);
                res.Data = model;
            }
            catch (Exception)
            {
                throw new Exception();
            }
            return res;
        }

        /// <summary>
        /// Sửa bản ghi
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<ServiceResponse> Edit(TModel model)
        {
            ServiceResponse res = new ServiceResponse();

            IDbConnection cnn = null;
            IDbTransaction transaction = null;

            try
            {
                if (model == null)
                {
                    res.OnError(ServiceResponseCode.InvalidData);
                    return res;
                }

                cnn = _dLBase.GetConnection();
                _dLBase.OpenConnection(cnn);
                transaction = cnn.BeginTransaction(IsolationLevel.ReadUncommitted);

                model.state = ModelState.Update;

                bool checkDuplicate = _dLBase.CheckDuplicate(model);
                if (checkDuplicate)
                {
                    res.OnError(ServiceResponseCode.Duplicate);
                    return res;
                }


                model.modified_by = "admin";
                model.modified_date = DateTimeUtility.GetNow();

                Dictionary<object, Exception> result = _dLBase.DoSaveBatchData(cnn, transaction, new List<TModel> { model }, ModelState.Update);

                if (result != null && result.Count > 0)
                {
                    res.OnError(ServiceResponseCode.Error);
                }
                else
                {
                    res.Data = model;
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                throw new Exception("Edit ex: " + Converter.Serialize(ex));
            }
            finally
            {
                _dLBase.CloseConnection(cnn);
            }

            return res;
        }
    }
}
