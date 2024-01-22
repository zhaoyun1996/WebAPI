using System.Collections;

namespace DemoWebAPI.Base.Model
{
    public class DBTypeName
    {
        public const string uuid = "uuid";
        public const string numeric = "numeric";
        public const string boolean = "boolean";
        public const string timestamp = "timestamp";
        public const string timestamp_with_time_zone = "timestamp_with_time_zone";
        public const string integer = "integer";
        public const string bigint = "bigint";
        public const string date = "date";
        public const string text = "text";
        public const string uuid_arr = "uuid_arr";
        public const string numeric_arr = "numeric_arr";
        public const string boolean_arr = "boolean_arr";
        public const string timestamp_arr = "timestamp_arr";
        public const string integer_arr = "integer_arr";
        public const string bigint_arr = "bigint_arr";
        public const string date_arr = "date_arr";
        public const string text_arr = "text_arr";
        public const string smallint = "smallint";
        public const string smallint_arr = "smallint_arr";
        public const string none = "";

        public static string GetDBTypeName(Type dataType, object value)
        {
            string dataTypeName = DBTypeName.none;
            if(dataType != null && dataType.IsArray)
            {
                Type subType;
                if(value != null && value is IList)
                {
                    subType = ((IList)value)[0].GetType();
                }
                else
                {
                    subType = dataType.GetElementType();
                }
                dataTypeName = GetDBTypeName(dataType).ToString();
            }
            return dataTypeName;
        }

        public static string GetDBTypeName(object value)
        {
            string dataTypeName;

            if(value != null)
            {
                Type typeName = value.GetType();
                dataTypeName = GetDBTypeName(typeName, value);
            }
            else
            {
                throw new Exception("Not support value is null");
            }
            return dataTypeName;
        }
    }
}
