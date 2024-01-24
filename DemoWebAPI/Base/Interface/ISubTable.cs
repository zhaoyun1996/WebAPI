namespace DemoWebAPI.Base.Interface
{
    public interface ISubTable
    {
        string GetQueryColumn();

        string GetTableName(bool ignoreView = false);

        string GetPrimaryKeyFieldName();
    }
}
