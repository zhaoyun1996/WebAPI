using Npgsql.Internal.Postgres;
using System.Collections;
using static DemoWebAPI.Constant.Enum;

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
                dataTypeName = GetDBTypeName(subType).ToString();
            }
            else
            {
                dataTypeName = GetDBTypeName(dataType).ToString();
            }
            return dataTypeName;
        }

        public static DBDataType GetDBTypeName(Type dataType)
        {
            DBDataType dataTypeName = DBDataType.none;
            if(dataType != null)
            {
                if(dataType == typeof(long) || dataType == typeof(long?))
                {
                    dataTypeName = DBDataType.bigint;
                }
                else if (dataType == typeof(Int32) || dataType == typeof(Int32?))
                {
                    dataTypeName = DBDataType.integer;
                }
                else if (dataType == typeof(Int16) || dataType == typeof(Int16?))
                {
                    dataTypeName = DBDataType.smallint;
                }
                else if (dataType == typeof(Int64) || dataType == typeof(Int64?))
                {
                    dataTypeName = DBDataType.bigint;
                }
                else if (dataType == typeof(Guid) || dataType == typeof(Guid?))
                {
                    dataTypeName = DBDataType.uuid;
                }
                else if (dataType == typeof(Boolean) || dataType == typeof(Boolean?))
                {
                    dataTypeName = DBDataType.boolean;
                }
                else if (dataType == typeof(Decimal) || dataType == typeof(Decimal?) || dataType == typeof(Double) || dataType == typeof(Double?))
                {
                    dataTypeName = DBDataType.bigint;
                }
                else if (dataType == typeof(DateTime) || dataType == typeof(DateTime?))
                {
                    dataTypeName = DBDataType.timestamp;
                }
                else if (dataType == typeof(string) || dataType == typeof(String))
                {
                    dataTypeName = DBDataType.text;
                }
                else if (dataType.IsEnum || (Nullable.GetUnderlyingType(dataType) != null && Nullable.GetUnderlyingType(dataType).IsEnum))
                {
                    dataTypeName = DBDataType.integer;
                }
                else
                {
                    throw new Exception($"DBTypeName/GetDBTypeName, not support dataType:{dataType.Name}");
                }
            }
            else
            {
                throw new Exception($"DBTypeName/GetDBTypeName, not support dataType is null");
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
