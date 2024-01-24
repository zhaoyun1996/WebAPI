using DemoWebAPI.Base.DL;
using DemoWebAPI.Base.Model;
using System.Data;
using System.Reflection;
using static DemoWebAPI.Constant.Enum;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace DemoWebAPI.Base.BL
{
    public partial class BLBase<TModel> where TModel : BaseModel
    {
        protected DLBase _dLBase = new DLBase();

        public virtual async Task<ServiceRespone> GetAll(string filter, string sort, string customFilter, string columns)
        {
            ServiceRespone res = new ServiceRespone();
            res.Data = this._dLBase.GetAll<TModel>(DatabaseType.Business, filter, sort, customFilter, columns);
            return res;
        }

        public virtual async Task<ServiceRespone> Insert(TModel model)
        {
            ServiceRespone res = new ServiceRespone();

            IDbConnection cnn = null;
            IDbTransaction transaction = null;

            try
            {
                if (model == null)
                {
                    res.OnError(ServiceResponseCode.InvalidData);
                    return res;
                }

                this._dLBase.CheckDuplicate(model);
                model.state = ModelState.Insert;
                model.SetAutoPrimaryKey();
                cnn = this._dLBase.GetConnection();
                this._dLBase.OpenConnection(cnn);

                transaction = cnn.BeginTransaction(IsolationLevel.ReadUncommitted);
                Dictionary<object, Exception> result = this._dLBase.DoSaveBatchData(cnn, transaction, new List<TModel> { model }, ModelState.Insert);

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
                this._dLBase.CloseConnection(cnn);
            }

            return res;
        }
    }
}
