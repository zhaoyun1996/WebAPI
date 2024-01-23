using DemoWebAPI.Base.DL;
using DemoWebAPI.Base.Model;
using System.Data;
using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.Base.BL
{
    public abstract class BLBase<TModel>
    {
        protected DLBase _dLBase = new DLBase();

        public virtual async Task<ServiceRespone> Insert(TModel model)
        {
            ServiceRespone res = new ServiceRespone();

            IDbConnection cnn = null;
            IDbTransaction transaction = null;

            try
            {
                if(model == null)
                {
                    res.OnError(ServiceResponseCode.InvalidData);
                    return res;
                }

                cnn = this._dLBase.GetConnection();
                this._dLBase.OpenConnection(cnn);

                transaction = cnn.BeginTransaction(IsolationLevel.ReadUncommitted);
                bool isSuccess = this._dLBase.InserData(cnn, transaction, model);
                transaction.Commit();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
