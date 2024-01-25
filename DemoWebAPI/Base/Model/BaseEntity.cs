using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using static DemoWebAPI.Base.Model.ModelAttribute;
using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.Base.Model
{
    public class BaseEntity
    {
        public object Clone()
        {
            return MemberwiseClone();
        }

        public string GetPrimaryKeyFieldName()
        {
            return ModelCoreHelper.GetPrimaryKeyFieldName(GetType());
        }

        public string GetTableName(bool ignoreView = false)
        {
            string tableName = "";
            var tableAttr = (TableAttribute)GetType().GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault();
            if (!ignoreView)
            {
                tableName = ((ViewAttribute)GetType().GetCustomAttributes(typeof(ViewAttribute), false).FirstOrDefault())?.ViewName;
            }
            if (tableAttr != null && string.IsNullOrEmpty(tableName))
            {
                tableName = $"{tableAttr.Name}";
            }
            if (tableName != null)
            {
                tableName = $"{tableAttr.Schema}.{tableName}";
            }

            return tableName;
        }

        public virtual object GetPrimaryKeyValue()
        {
            return GetValueByAttribute(typeof(KeyAttribute));
        }

        public object GetValueByAttribute(Type attrType)
        {
            PropertyInfo[] props = MemoryCacheService.GetPropertyInfo(GetType());
            PropertyInfo oPropertyInfoKey = null;
            if (props != null)
            {
                oPropertyInfoKey = props.SingleOrDefault(p => p.GetCustomAttribute(attrType, true) != null);
            }
            if (oPropertyInfoKey != null)
            {
                return oPropertyInfoKey.GetValue(this);
            }
            return null;
        }

        public virtual EnumDataType GetPrimaryKeyFieldType()
        {
            return EnumDataType.Guid;
        }

        public TableAttribute GetTableAttribute()
        {
            return ModelCoreHelper.GetTableAttribute(GetType());
        }

        public string GetTableNameOnly()
        {
            return ModelCoreHelper.GetDBTableName(this);
        }

        public string GetSchemaName()
        {
            return ModelCoreHelper.GetDBSchemaName(this);
        }

        public string GetFieldName(Type attrType)
        {
            return ModelCoreHelper.GetFieldName(GetType(), attrType);
        }

        [IgnoreDataMember()]
        [JsonIgnore]
        [NotMapped]
        public object this[string propertyName]
        {
            get
            {
                PropertyInfo pi = GetType().GetProperty(propertyName);
                if (pi != null)
                {
                    return pi.GetValue(this, null);
                }
                else
                {
                    throw new Exception();
                }
            }
            set
            {
                PropertyInfo pi = GetType().GetProperty(propertyName);
                if (pi != null)
                {
                    pi.SetValue(this, value, null);
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        public bool ContainProperty(string property)
        {
            PropertyInfo pi = GetType().GetProperty(property);
            if (pi == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
