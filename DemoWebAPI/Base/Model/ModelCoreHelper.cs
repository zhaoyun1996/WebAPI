using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;
using System.Reflection;
using DemoWebAPI.Base.Interface;
using Newtonsoft.Json;
using static DemoWebAPI.Base.Model.ModelAttribute;
using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.Base.Model
{
    public class ModelCoreHelper
    {
        public static Dictionary<object, T> ConvertToDictionaryByID<T>(List<T> data) where T : BaseEntity
        {
            Dictionary<object, T> result = new Dictionary<object, T>();
            if (data != null && data.Count > 0)
            {
                foreach (T item in data)
                {
                    object key = item.GetPrimaryKeyValue();
                    if (result.ContainsKey(key))
                    {
                        result.Add(key, item);
                    }
                }
            }
            return result;
        }

        public static string GetPrimaryKeyValue<T>()
        {
            var t = typeof(T);
            string keyName = GetFieldName(t, typeof(KeyAttribute));
            return keyName;
        }

        public static string GetPrimaryKeyFieldName(Type modelType)
        {
            string keyName = GetFieldName(modelType, typeof(KeyAttribute));
            return keyName;
        }

        public static string GetFieldName(Type modelType, Type attrType)
        {
            string fieldName = string.Empty;
            PropertyInfo[] props = modelType.GetProperties();
            if (props != null)
            {
                var propertyInfoKey = props.SingleOrDefault(p => p.GetCustomAttribute(attrType, true) != null);
                if (propertyInfoKey != null)
                {
                    fieldName = propertyInfoKey.Name;
                }
            }
            return fieldName;
        }

        public static Dictionary<string, object> GetDynamicObject()
        {
            var jsonString = @"{}";
            var jsonDynamic = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            return jsonDynamic;
        }

        public static string GetDBTableName(Type t, string table = "")
        {
            string tableName = table;
            if (string.IsNullOrEmpty(tableName))
            {
                TableAttribute tableAttribute = GetTableAttribute(t);
                if (tableAttribute != null)
                {
                    tableName = tableAttribute.Name;
                }
                if (string.IsNullOrEmpty(tableName) && t != null)
                {
                    tableName = t.Name;
                }
            }

            return tableName;
        }

        public static string GetDBTableName(object item, string table = "")
        {
            string tableName = table;
            if (string.IsNullOrEmpty(tableName) && item != null)
            {
                tableName = GetDBTableName(item.GetType(), table);
            }

            return tableName;
        }

        public static TableAttribute GetTableAttribute(Type t)
        {
            return (TableAttribute)t.GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault();
        }

        public static string GetDBSchemaName(Type t, string schema = "")
        {
            string schemaName = schema;
            if (string.IsNullOrEmpty(schemaName))
            {
                TableAttribute tableAttribute = GetTableAttribute(t);
                if (tableAttribute != null)
                {
                    schemaName = tableAttribute.Schema;
                }
            }
            return schemaName;
        }

        public static string GetDBSchemaName(object item, string schema = "")
        {
            string schemaName = schema;
            if (string.IsNullOrEmpty(schemaName) && item != null)
            {
                schemaName = GetDBSchemaName(item.GetType(), schema);
            }
            if (string.IsNullOrEmpty(schemaName))
            {
                schemaName = DBConstant.mscDefaultSchema;
            }
            return schemaName;
        }

        public static string GetDBPrimaryKeyName(Type t, string primaryKey = "")
        {
            string primaryKeyName = primaryKey;
            if (string.IsNullOrEmpty(primaryKey))
            {
                primaryKeyName = GetPrimaryKeyFieldName(t);
            }
            return primaryKeyName;
        }

        public static string GetDBPrimaryKeyName(object item, string primaryKey = "")
        {
            string primaryKeyName = primaryKey;
            if (string.IsNullOrEmpty(primaryKey) && item != null)
            {
                primaryKeyName = GetDBPrimaryKeyName(item.GetType(), primaryKey);
            }
            return primaryKeyName;
        }

        public static bool IsDBJsonField(PropertyInfo prop)
        {
            var cTypeAttrs = prop.GetCustomAttributes(typeof(ColumnAttribute), false);
            if (cTypeAttrs.Length > 0)
            {
                var cTypeAttr = cTypeAttrs.First() as ColumnAttribute;
                switch (cTypeAttr.TypeName)
                {
                    case "json":
                    case "jsonb":
                        return true;
                }
            }
            return false;
        }

        public static bool IsDBTSvectorField(PropertyInfo prop)
        {
            var cTypeAttrs = prop.GetCustomAttributes(typeof(ColumnAttribute), false);
            if (cTypeAttrs.Length > 0)
            {
                var cTypeAttr = cTypeAttrs.First() as ColumnAttribute;
                switch (cTypeAttr.TypeName)
                {
                    case "tsvector":
                        return true;
                }
            }
            return false;
        }

        public static Dictionary<string, PropertyInfo> GetFieldCanWrite(Type type)
        {
            Dictionary<string, PropertyInfo> columnMappings = new Dictionary<string, PropertyInfo>(StringComparer.InvariantCultureIgnoreCase);
            PropertyInfo[] props = MemoryCacheService.GetPropertyInfo(type);
            foreach (PropertyInfo pi in props)
            {
                if (pi.CanWrite)
                {
                    if (columnMappings.ContainsKey(pi.Name))
                    {
                        if (pi.DeclaringType == type)
                        {
                            columnMappings[pi.Name] = pi;
                        }
                    }
                    else
                    {
                        columnMappings.Add(pi.Name, pi);
                    }
                }
            }

            return columnMappings;
        }

        public static Dictionary<string, PropertyInfo> GetFieldForInsertUpdate(Type type, ModelState modelState, string primaryKey = "")
        {
            Dictionary<string, PropertyInfo> columnMappings = new Dictionary<string, PropertyInfo>(StringComparer.InvariantCultureIgnoreCase);
            PropertyInfo[] props = MemoryCacheService.GetPropertyInfo(type);
            IEnumerable<PropertyInfo> propsInsertUpdate = null;
            switch (modelState)
            {
                case ModelState.Insert:
                    propsInsertUpdate = props.Where(p => p.GetCustomAttribute(typeof(NotMappedAttribute)) == null && p.GetCustomAttribute(typeof(DatabaseGeneratedAttribute)) == null);
                    break;
                case ModelState.Update:
                    propsInsertUpdate = props.Where(p => p.Name == primaryKey || (p.GetCustomAttribute(typeof(NotMappedAttribute)) == null && p.GetCustomAttribute(typeof(DatabaseGeneratedAttribute)) == null && p.GetCustomAttribute(typeof(UpdateIgnoreAttribute)) == null));
                    break;
                default:
                    propsInsertUpdate = props.Where(p => p.GetCustomAttribute(typeof(NotMappedAttribute)) == null);
                    break;
            }
            if (propsInsertUpdate != null)
            {
                foreach (PropertyInfo pi in propsInsertUpdate)
                {
                    if (pi.CanWrite)
                    {
                        if (columnMappings.ContainsKey(pi.Name))
                        {
                            if (pi.DeclaringType == type)
                            {
                                columnMappings[pi.Name] = pi;
                            }
                        }
                        else
                        {
                            columnMappings[pi.Name] = pi;
                        }
                    }
                }
            }
            return columnMappings;
        }

        public static object GetPropertyValue<T>(T t, string fieldName)
        {
            PropertyInfo pi = t.GetType().GetProperty(fieldName);
            if(pi != null)
            {
                return pi.GetValue(t, null);
            }
            else
            {
                throw new Exception($"<{fieldName}> does not exists in object <{t.GetType().ToString()}>");
            }
        }

        public static void SetPropertyValue<T>(T t, string fieldName, object value)
        {
            PropertyInfo pi = t.GetType().GetProperty(fieldName);
            if (pi != null)
            {
                pi.SetValue(t, value, null);
            }
            else
            {
                throw new Exception($"<{fieldName}> does not exists in object <{t.GetType().ToString()}>");
            }
        }

        public static string JoinColumnValues<T>(T t, List<string> fieldNames, string splitKey = ".")
        {
            if(fieldNames != null && fieldNames.Count > 0)
            {
                return string.Join(splitKey, fieldNames.Select(i => GetPropertyValue(t, i)));
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
