using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.Base.Model
{
    public interface IDBParam
    {
        string ParameterName { get; set; }

        string DataTypeName { get; set; }

        object Value { get; set; }
    }

    public class DBParam : IDBParam
    {
        DBDataType _DBDataType = DBDataType.none;

        public DBParam(string parameterName, DBDataType dataType, object value)
        {
            this.ParameterName = parameterName;
            this.DBDataType = dataType;
            this.Value = value;
        }

        public DBParam(string parameterName, string dataTypeName, object value)
        {
            this.ParameterName = parameterName;
            this.DBDataType = dataTypeName;
            this.Value = value;
        }

        public string ParameterName { get; set; }

        public string DataTypeName { get; set; }

        public object Value { get; set; }

        public DBDataType DBDataType
        {
            get { return _DBDataType; }
            set
            {
                _DBDataType = value;
                switch(_DBDataType)
                {
                    case DBDataType.uuid: DataTypeName = DBTypeName.uuid; break;
                    case DBDataType.numeric: DataTypeName = DBTypeName.numeric; break;
                    case DBDataType.boolean: DataTypeName = DBTypeName.boolean; break;
                    case DBDataType.timestamp: DataTypeName = DBTypeName.timestamp; break;
                    case DBDataType.timestamp_with_time_zone: DataTypeName = DBTypeName.timestamp_with_time_zone; break;
                    case DBDataType.integer: DataTypeName = DBTypeName.integer; break;
                    case DBDataType.bigint: DataTypeName = DBTypeName.bigint; break;
                    case DBDataType.date: DataTypeName = DBTypeName.date; break;
                    case DBDataType.text: DataTypeName = DBTypeName.text; break;
                    case DBDataType.uuid_arr: DataTypeName = DBTypeName.uuid_arr; break;
                    case DBDataType.numeric_arr: DataTypeName = DBTypeName.numeric_arr; break;
                    case DBDataType.boolean_arr: DataTypeName = DBTypeName.boolean_arr; break;
                    case DBDataType.timestamp_arr: DataTypeName = DBTypeName.timestamp_arr; break;
                    case DBDataType.integer_arr: DataTypeName = DBTypeName.integer_arr; break;
                    case DBDataType.bigint_arr: DataTypeName = DBTypeName.bigint_arr; break;
                    case DBDataType.date_arr: DataTypeName = DBTypeName.date_arr; break;
                    case DBDataType.text_arr: DataTypeName = DBTypeName.text_arr; break;
                    case DBDataType.smallint: DataTypeName = DBTypeName.smallint; break;
                    case DBDataType.smallint_arr: DataTypeName = DBTypeName.smallint_arr; break;
                }
            }
        }
    }
}
