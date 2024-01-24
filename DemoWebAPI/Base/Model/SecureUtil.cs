namespace DemoWebAPI.Base.Model
{
    public class SecureUtil
    {
        public static string SafeSqlLiteral(string inputSql)
        {
            if(string.IsNullOrEmpty(inputSql))
            {
                return inputSql;
            }
            return inputSql.Replace("'", "''");
        }
    }
}
