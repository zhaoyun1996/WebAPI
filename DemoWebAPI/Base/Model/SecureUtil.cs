using System.Text.RegularExpressions;

namespace DemoWebAPI.Base.Model
{
    public class SecureUtil
    {
        private static readonly Regex regSystemThreats = new Regex("\\s?;\\s?|\\s?drop \\s|\\s?grant \\s|^'|\\s?--|\\s?union \\s|\\s?delete \\s|\\s?update \\s|\\s?truncate \\s|\\s?sysobjects\\s?|\\s?xp_.*?|\\s?syslogins\\s?|\\s?sysremote\\s?|\\s?sysusers\\s?|\\s?sysxlogins\\s?|\\s?sysdatabases\\s?|\\s?aspnet_.*?|\\s?exec \\s?|\\s?execute \\s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
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
