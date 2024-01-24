using System.Text.RegularExpressions;

namespace DemoWebAPI.Base.Model
{
    public class SecureUtil
    {
        /// <summary>
        /// TODO
        /// </summary>
        private static readonly Regex regSystemThreats = new Regex("");
        public static string SafeSqlLiteral(string inputSql)
        {
            if(string.IsNullOrEmpty(inputSql))
            {
                return inputSql;
            }
            return inputSql.Replace("'", "''");
        }

        public static bool DetectSqlInjection(string inputSql)
        {
            return !string.IsNullOrWhiteSpace(inputSql) && SecureUtil.regSystemThreats.IsMatch(inputSql);
        }
    }
}
