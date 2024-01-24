using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.Base.Model
{
    public class ScriptHelper
    {
        const string mscProcSelectByID = "{0}.proc_select_{1}_by_id";
        const string mscProcInsert = "{0}.proc_insert_{1}_by_id";
        const string mscProcUpdate = "{0}.proc_update_{1}_by_id";
        const string mscProcDelete = "{0}.proc_delete_{1}_by_id";

        private static List<string> _specialColumns = new List<string>() { "xmin as edit_version", "1" };

        public static string GetColumnScript(string columnNames, Dictionary<string, string> serverMappingColumns = null)
        {
            if (!string.IsNullOrEmpty(columnNames))
            {
                if (serverMappingColumns != null && serverMappingColumns.Count > 0)
                {
                    return string.Join(",", columnNames.Split(',').Select(t => $"{RemoveInjectionColumnName(t, serverMappingColumns)} as {RemoveInjectionColumnName(t)}"));
                }
                else
                {
                    return string.Join(",", columnNames.Split(',').Select(t => RemoveInjectionColumnName(t)));
                }
            }
            else
            {
                return columnNames;
            }
        }

        public static string GetColumnScript(List<string> columnNames, Dictionary<string, string> serverMappingColumns = null)
        {
            if (columnNames != null && columnNames.Count > 0)
            {
                if (serverMappingColumns != null && serverMappingColumns.Count > 0)
                {
                    return string.Join(",", columnNames.Select(t => $"{RemoveInjectionColumnName(t, serverMappingColumns)} as {RemoveInjectionColumnName(t)}"));
                }
                else
                {
                    return string.Join(",", columnNames.Select(t => RemoveInjectionColumnName(t)));
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public static string RemoveInjectionColumnOrTable(string data, string objectName = "cột")
        {
            string injectionColumnName = "\"";
            if (!string.IsNullOrEmpty(data))
            {
                string result = $"{injectionColumnName}{data.Replace("'", "").Trim().Replace(injectionColumnName, "")}{injectionColumnName}";
                if (result.Contains(" ") || result.Contains(";"))
                {
                    throw new Exception($"[sqlInjection]: tên {objectName} không hợp lệ ({data})");
                }
                else
                {
                    return result;
                }
            }
            else
            {
                return data;
            }
        }

        public static string RemoveInjectionTableOrViewName(string tableName)
        {
            return RemoveInjectionColumnOrTable(tableName, "bảng/view");
        }

        public static string RemoveInjectionColumnName(string columnName, Dictionary<string, string> serverMappingColumns = null)
        {
            if (!string.IsNullOrEmpty(columnName) && !_specialColumns.Contains(columnName.Trim()))
            {
                if (serverMappingColumns != null && serverMappingColumns.ContainsKey(columnName))
                {
                    return serverMappingColumns[columnName];
                }
                else
                {
                    return RemoveInjectionColumnOrTable(columnName, "cột");
                }
            }
            else
            {
                return columnName;
            }
        }

        //public static ScriptHelperOutputSaveBatch GenerateScript<T>(List<T> lstModel, ModelState modelState = ModelState.None, string schema = "", string tableName = "", string primaryKey = "", string userName = "", string sufixParamName = "", Guid? userID = null)
        //{
        //    ScriptHelperOutputSaveBatch tsql = null;

        //    switch (modelState)
        //    {
        //        case ModelState.Insert:
        //            tsql = GenerateInsertScriptWithUnnest(lstModel, schema, tableName, userName: userName, sufixParamName, userID: userID);
        //            break;
        //        case ModelState.Update:
        //            tsql = GenerateInsertScriptWithUnnest(lstModel, schema, tableName, null, sufixParamName);
        //            break;
        //        default:
        //            throw new Exception($"ScriptHelper/GenerateScript not support modelState = {modelState.ToString()}");
        //    }

        //    return tsql;
        //}

        public static ScriptHelperOutputSaveBatch GetSaveBatchInput<T>(List<T> lstModel, ModelState modelState = ModelState.None, string primaryKey = "", List<string> paramColumnNames = null, string userName = "", string sufixParamName = "", Guid? userID = null)
        {
            ScriptHelperOutputSaveBatch script = new ScriptHelperOutputSaveBatch();
            if (lstModel != null && lstModel.Count > 0)
            {
                Dictionary<PropertyInfo, List<object>> data = new Dictionary<PropertyInfo, List<object>>();
                Dictionary<string, PropertyInfo> allFields = ModelCoreHelper.GetFieldForInsertUpdate(lstModel[0].GetType(), modelState, primaryKey);
                if (paramColumnNames != null && paramColumnNames.Count > 0)
                {
                    if (!string.IsNullOrEmpty(primaryKey) && !paramColumnNames.Contains(primaryKey))
                    {
                        paramColumnNames.Add(primaryKey);
                    }
                    allFields = allFields.Where(t => paramColumnNames.Contains(t.Key)).ToDictionary(t => t.Key, t => t.Value);
                    if (allFields != null && allFields.Count > 0)
                    {
                        Dictionary<PropertyInfo, List<object>> dataParam = allFields.ToDictionary(t => t.Value, t => lstModel.Select(o => GetValueDefault(t.Value.Name, modelState, userName, t.Value.GetValue(o), userID)).ToList());
                        foreach (KeyValuePair<string, PropertyInfo> col in allFields)
                        {
                            script.AppendFieldNotNulls($"{col.Key}{sufixParamName}", col.Value);
                            script.AppendFieldBuildScripts($"{col.Key}{sufixParamName}", col.Value);
                        }
                        script.AppendParam(dataParam, sufixParamName);
                    }
                }
                else
                {
                    if (allFields != null && allFields.Count > 0)
                    {
                        Dictionary<string, PropertyInfo> fieldData = new Dictionary<string, PropertyInfo>();
                        if (modelState == ModelState.Update)
                        {
                            foreach (T o in lstModel)
                            {
                                Dictionary<string, PropertyInfo> fieldDataChange = GetPropChangeData(allFields, primaryKey, o);
                                foreach (var item in fieldDataChange)
                                {
                                    fieldData[item.Key] = item.Value;
                                }
                            }
                        }
                        else
                        {
                            fieldData = allFields;
                        }
                        if (fieldData != null && fieldData.Count > 0)
                        {
                            Dictionary<PropertyInfo, List<object>> dataParam = fieldData.ToDictionary(t => t.Value, t => new List<object>());
                            foreach (T o in lstModel)
                            {
                                foreach (KeyValuePair<PropertyInfo, List<object>> col in dataParam)
                                {
                                    string columnName = col.Key.Name;
                                    object value = GetValueDefault(columnName, modelState, userName, col.Key.GetValue(o), userID);
                                    if (value != null)
                                    {
                                        script.AppendFieldNotNulls($"{columnName}{sufixParamName}", col.Key);
                                    }
                                    script.AppendFieldBuildScripts($"{columnName}{sufixParamName}", col.Key);
                                    col.Value.Add(value);
                                }
                            }
                            script.AppendParam(dataParam, sufixParamName);
                        }
                    }
                }
            }

            return script;
        }

        public static object GetValueDefault(string fieldName, ModelState modelState, string userName, object value, Guid? userID = null)
        {
            if (value == null || (value is string && string.IsNullOrEmpty((string)value)) || (value is String && string.IsNullOrEmpty((String)value)))
            {
                switch (fieldName)
                {
                    case ColumnNameCore.modified_by:
                        if(!string.IsNullOrEmpty(userName))
                        {
                            value = userName;
                        }
                        break;
                    case ColumnNameCore.created_by:
                        if (modelState == ModelState.Insert && !string.IsNullOrEmpty(userName))
                        {
                            value = userName;
                        }
                        break;
                    case ColumnNameCore.modified_date:
                        value = DateTime.Now;
                        break;
                    case ColumnNameCore.created_date:
                        if (modelState == ModelState.Insert)
                        {
                            value = DateTime.Now;
                        }
                        break;
                }
            }
            else if(value is Enum)
            {
                value = (int)value;
            }

            return value;
        }

        public static ScriptHelperOutputDBParam GenerateDeleteScript(Type t, object[] id, string schema = "", string tableName = "", string primaryKeyName = "", string sufixParamName = "")
        {
            ScriptHelperOutputDBParam scriptHelper = new ScriptHelperOutputDBParam();
            if(t != null)
            {
                string sql = string.Empty;
                if(!string.IsNullOrEmpty(schema) && !string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(primaryKeyName))
                {
                    sql = $"delete from {schema}.{tableName} where {primaryKeyName}=any(:ids);";
                }
                else
                {
                    if(string.IsNullOrEmpty(sql))
                    {
                        schema = ModelCoreHelper.GetDBSchemaName(t, schema);
                        tableName = ModelCoreHelper.GetDBSchemaName(t, tableName);
                        primaryKeyName = ModelCoreHelper.GetDBSchemaName(t, primaryKeyName);

                        if(!string.IsNullOrEmpty(primaryKeyName))
                        {
                            sql = $"delete from {schema}.{tableName} where {primaryKeyName}=any(:ids);";
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
                if(!string.IsNullOrEmpty(sufixParamName))
                {
                    sql = sql.Replace("any(:ids)", $"any(:ids{sufixParamName})");
                }
                scriptHelper.AppendScript(sql);
                scriptHelper.AppendParam($"ids{sufixParamName}", DBTypeName.none, id);
            }
            return scriptHelper;
        }

        public static ScriptHelperOutputDBParam GenerateScript<T>(List<T> lstModel, ModelState modelState = ModelState.None, string schema = "", string tableName = "", string userName = "", string primaryKey = "", string sufixParamName = "", Guid? userID = null)
        {
            ScriptHelperOutputDBParam tsql = null;

            switch (modelState)
            {
                case ModelState.Insert:
                    tsql = GenerateInsertScriptWithUnnest(lstModel, schema, tableName, userName: userName, sufixParamName, userID: userID);
                    break;
                case ModelState.Update:
                    tsql = GenerateUpdateScriptWithUnnest(lstModel, schema, tableName, primaryKey, null, userName: userName, sufixParamName);
                    break;
                default:
                    throw new Exception();
                    break;
            }

            return tsql;
        }

        public static ScriptHelperOutputSaveBatch GenerateInsertScriptWithUnnest<T>(List<T> lstModel, string schema = "", string tableName = "", string userName = "", string sufixParamName = "", Guid? userID = null)
        {
            ScriptHelperOutputSaveBatch scriptHelper = new ScriptHelperOutputSaveBatch();
            if(lstModel != null && lstModel.Count > 0)
            {
                scriptHelper = GetSaveBatchInput(lstModel, ModelState.Insert, userName: userName, userID: userID, sufixParamName: sufixParamName);
                if (scriptHelper.Param != null && scriptHelper.Param.Count > 0)
                {
                    Dictionary<string, string> sField = scriptHelper.GetFieldAndParam(ModelState.Insert, sufixParamName: sufixParamName);
                    schema = ModelCoreHelper.GetDBSchemaName(lstModel[0], schema);
                    tableName = ModelCoreHelper.GetDBTableName(lstModel[0], tableName);
                    string sql = $"insert into {schema}.{tableName} ({string.Join($",{Environment.NewLine}", sField.Keys)}){Environment.NewLine} select {string.Join($",{Environment.NewLine}", sField.Values.Select(t => t.Replace("@", "")))} from {Environment.NewLine} ({scriptHelper.GetUnnestTableScript()}) a;";
                    scriptHelper.AppendScript(sql);
                }
            }
            return scriptHelper;
        }

        public static ScriptHelperOutputSaveBatch GenerateUpdateScriptWithUnnest<T>(List<T> lstModel, string schema = "", string tableName = "", string primaryKey = "", List<string> updateColumnNames = null, string userName = "", string sufixParamName = "")
        {
            ScriptHelperOutputSaveBatch scriptHelper = new ScriptHelperOutputSaveBatch();
            if (lstModel != null && lstModel.Count > 0)
            {
                primaryKey = ModelCoreHelper.GetDBPrimaryKeyName(lstModel[0], primaryKey);
                scriptHelper = GetSaveBatchInput(lstModel, ModelState.Update, primaryKey, updateColumnNames, userName: userName, sufixParamName: sufixParamName);
                if (scriptHelper.Param != null && scriptHelper.Param.Count > 0)
                {
                    Dictionary<string, string> sField = scriptHelper.GetFieldAndParam(ModelState.Update, primaryKey, sufixParamName: sufixParamName);
                    schema = ModelCoreHelper.GetDBSchemaName(lstModel[0], schema);
                    tableName = ModelCoreHelper.GetDBTableName(lstModel[0], tableName);
                    if(!string.IsNullOrEmpty(primaryKey))
                    {
                        string sql = $"update {schema}.{tableName} a set {Environment.NewLine} {string.Join($",{Environment.NewLine}", sField.Select(t => $"{t.Key}={t.Value.Replace("@", "b.")}"))}{Environment.NewLine} from ({scriptHelper.GetUnnestTableScript()}) b {Environment.NewLine} where a.{primaryKey} = b.{primaryKey}{sufixParamName};";
                        scriptHelper.AppendScript(sql);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
            return scriptHelper;
        }

        private static Dictionary<string, PropertyInfo> GetPropChangeData(Dictionary<string, PropertyInfo> props, string primaryKey, object data)
        {
            Dictionary<string, PropertyInfo> fields = new Dictionary<string, PropertyInfo>();
            BaseEntity model = null;
            if(data is BaseEntity)
            {
                model = (BaseEntity)data;
            }
            if (model != null && !string.IsNullOrEmpty(primaryKey) && model.ContainProperty(ColumnNameCore.old_data) && model[ColumnNameCore.old_data] != null
                && model[ColumnNameCore.old_data].GetType().FullName == model.GetType().FullName)
            {
                foreach (KeyValuePair<string, PropertyInfo> item in props)
                {
                    PropertyInfo pro = item.Value;
                    object oldValue = pro.GetValue(model[ColumnNameCore.old_data]);
                    object newValue = pro.GetValue(model);
                    if (pro.Name == ColumnNameCore.created_date
                        || pro.Name == ColumnNameCore.created_by
                        || pro.Name == ColumnNameCore.modified_date
                        || pro.Name == ColumnNameCore.modified_by)
                    {
                        continue;
                    }
                    if ((pro.PropertyType == typeof(DateTime) || pro.PropertyType == typeof(DateTime?))
                        && oldValue != null
                        && newValue != null
                        && DateTime.Parse(oldValue?.ToString()).Date != DateTime.Parse(newValue?.ToString()).Date
                        || (newValue != null && oldValue == null)
                        || (newValue == null && oldValue != null)
                        || (newValue != null && oldValue != null && !newValue.ToString().Equals(oldValue.ToString())))
                    {
                        fields.Add(item.Key, item.Value);
                    }
                }
                if(fields != null && fields.Count > 0)
                {
                    if(props.ContainsKey(ColumnNameCore.modified_by)) {
                        PropertyInfo pro = props[ColumnNameCore.modified_by];
                        fields.Add(ColumnNameCore.modified_by, pro);
                    }

                    if (props.ContainsKey(ColumnNameCore.modified_date))
                    {
                        PropertyInfo pro = props[ColumnNameCore.modified_date];
                        fields.Add(ColumnNameCore.modified_date, pro);
                    }

                    if (props.ContainsKey(primaryKey))
                    {
                        PropertyInfo pro = props[primaryKey];
                        fields.Add(primaryKey, pro);
                    }
                }
            }
            else
            {
                fields = props;
            }
            return fields;
        }
    }
}
