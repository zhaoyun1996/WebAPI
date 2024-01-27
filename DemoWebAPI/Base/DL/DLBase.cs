using DemoWebAPI.Base.Interface;
using DemoWebAPI.Base.Model;
using Npgsql;
using System.Data;
using System.Collections;
using System.Reflection;
using static DemoWebAPI.Constant.Enum;
using System.ComponentModel.DataAnnotations;
using static DemoWebAPI.Base.Model.ModelAttribute;
using Dapper;
using DemoWebAPI.Constant;
using System.Net;
using System.ComponentModel.Design;
using System.Transactions;

namespace DemoWebAPI.Base.DL
{
    public partial class DLBase
    {
        protected PostgreSQLProvider _PostgreSQLProvider = new PostgreSQLProvider();
        private Dictionary<string, Type> _types = new Dictionary<string, Type>();
        private object syncLockTypes = new object();

        /// <summary>
        /// Tạo kết nối
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetConnection()
        {
            string connectionString = GetConnectionString();
            NpgsqlConnection cnn = new NpgsqlConnection(connectionString);

            return cnn;
        }

        /// <summary>
        /// Mở kết nối đến DB
        /// </summary>
        /// <returns></returns>
        public void OpenConnection(IDbConnection cnn)
        {
            if (cnn.State != ConnectionState.Open)
            {
                cnn.Open();
            }
        }

        /// <summary>
        /// Thực hiện thêm dữ liệu
        /// </summary>
        /// <returns></returns>
        public Dictionary<object, Exception> DoSaveBatchData<T>(IDbConnection cnn, IDbTransaction transaction, List<T> lstModel, ModelState modelState, ScriptHelperOutputDBParam scriptCustomBefore = null, ScriptHelperOutputDBParam scriptCustomAfter = null, bool allowException = false, string schema = "", string tableName = "", string primaryKey = "", List<string> updateColumns = null, List<string> updateByColumnsNames = null, int commandTimeout = 30)
        {
            Dictionary<object, Exception> exceptionData = new Dictionary<object, Exception>();

            try
            {
                if (lstModel != null && lstModel.Count > 0)
                {
                    schema = ModelCoreHelper.GetDBSchemaName(lstModel[0], schema);
                    tableName = ModelCoreHelper.GetDBTableName(lstModel[0], tableName);
                    primaryKey = ModelCoreHelper.GetDBPrimaryKeyName(lstModel[0], primaryKey);
                    if (transaction != null)
                    {
                        allowException = false;
                    }

                    if (lstModel != null && lstModel.Count > 0)
                    {
                        int page = 0;
                        int pageSize = 10000;

                        while (page * pageSize <= lstModel.Count)
                        {
                            var pagingData = lstModel.Skip(page * pageSize).Take(pageSize).ToList();

                            if (pagingData.Count > 0)
                            {
                                IDbTransaction tran = transaction;

                                try
                                {
                                    ScriptHelperOutputDBParam scriptCustomBeforeCurrent = null;
                                    ScriptHelperOutputDBParam scriptCustomAfterCurrent = null;

                                    if (page == 0)
                                    {
                                        scriptCustomBeforeCurrent = scriptCustomBefore;
                                    }
                                    if (page * pageSize <= lstModel.Count)
                                    {
                                        scriptCustomAfterCurrent = scriptCustomAfter;
                                    }
                                    if (!allowException)
                                    {
                                        DoSaveData(cnn, tran, pagingData, modelState, scriptCustomBeforeCurrent, scriptCustomAfterCurrent, schema, tableName, primaryKey, updateColumns, updateByColumnsNames, commandTimeout);
                                    }
                                    else
                                    {
                                        if (transaction == null)
                                        {
                                            tran = cnn.BeginTransaction();
                                        }
                                        DoSaveData(cnn, tran, pagingData, modelState, scriptCustomAfterCurrent, scriptCustomAfterCurrent, schema, tableName, primaryKey, updateColumns, updateByColumnsNames, commandTimeout);
                                        if (transaction == null)
                                        {
                                            tran.Commit();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (!allowException)
                                    {
                                        throw ex;
                                    }
                                    else
                                    {
                                        if (tran != null && transaction == null)
                                        {
                                            tran.Rollback();
                                        }
                                        if (tran != null && transaction == null)
                                        {
                                            tran.Dispose();
                                        }
                                        tran = null;
                                        if (pagingData.Count > 1)
                                        {
                                            if (scriptCustomBefore != null)
                                            {
                                                ExecuteNonQuery(CommandType.Text, cnn, tran, scriptCustomBefore.Script, scriptCustomBefore.Param, commandTimeout);
                                            }
                                            for (int i = 0; i < pagingData.Count; i++)
                                            {
                                                var rowData = pagingData[i];
                                                try
                                                {
                                                    DoSaveData(cnn, tran, new List<T> { rowData }, modelState, null, null, schema, tableName, primaryKey, updateColumns, updateByColumnsNames, commandTimeout);
                                                }
                                                catch (Exception ex2)
                                                {
                                                    if (!exceptionData.ContainsKey((T)rowData))
                                                        exceptionData.Add((T)rowData, ex2);
                                                }
                                            }
                                            if (scriptCustomAfter != null)
                                            {
                                                ExecuteNonQuery(CommandType.Text, cnn, tran, scriptCustomAfter.Script, scriptCustomAfter.Param, commandTimeout);
                                            }
                                        }
                                        else
                                        {
                                            if (!exceptionData.ContainsKey((T)pagingData[0]))
                                                exceptionData.Add((T)pagingData[0], ex);
                                        }
                                    }
                                }
                                finally
                                {
                                    if (tran != null && transaction == null)
                                        tran.Dispose();
                                    tran = null;
                                }
                            }

                            page += 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!allowException)
                    throw ex;
            }

            return exceptionData;
        }

        public Dictionary<object, Exception> DoSaveBatchData<T>(List<T> lstModel, ModelState modelState, ScriptHelperOutputDBParam scriptCustomBefore = null, ScriptHelperOutputDBParam scriptCustomAfter = null, bool allowException = false, string schema = "", string tableName = "", string primaryKey = "", List<string> updateColumns = null, List<string> updateByColumnsNames = null, int commandTimeout = 30)
        {
            Dictionary<object, Exception> result = new Dictionary<object, Exception>();
            var cnn = GetConnection();
            try
            {
                OpenConnection(cnn);
                result = DoSaveBatchData(cnn, null, lstModel, modelState, scriptCustomBefore, scriptCustomAfter, allowException, schema, tableName, primaryKey, null, updateByColumnsNames, commandTimeout);
            }
            finally
            {
                CloseConnection(cnn);
            }
            return result;
        }

        public virtual int ExecuteNonQuery(CommandType commandType, IDbConnection cnn, IDbTransaction transaction, string sql, object param, int commandTimeout = DBConstant.CommonTimeoutNon)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(sql))
            {
                if (IsSupportDBParam(param))
                {
                    IDbCommand cmd = CreateCommand(cnn, sql, param, commandType, commandTimeout);
                    result = cmd.ExecuteNonQuery();
                }
                else
                {
                    result = _PostgreSQLProvider.Execute(cnn, sql, param, transaction, GetCommandTimeout(commandTimeout), commandType);
                }
            }
            return result;
        }

        public void SaveData<T>(IDbConnection cnn, IDbTransaction transaction, List<T> insertData, List<T> updateData, List<T> deleteData, ScriptHelperOutputDBParam scriptCustomBefore, ScriptHelperOutputDBParam scriptCustomAfter, string schema, string tableName, string primaryKey, int commandTimeout)
        {
            if ((updateData != null && updateData.Count > 0) || (insertData != null && insertData.Count > 0) || (deleteData != null && deleteData.Count > 0))
            {
                ScriptHelperOutputSaveBatch tsql = new ScriptHelperOutputSaveBatch();
                tsql.Append(scriptCustomBefore);
                if (deleteData != null && deleteData.Count > 0)
                {
                    object[] ids;
                    if (deleteData[0] is BaseEntity)
                    {
                        ids = deleteData.Select(t => ((BaseEntity)(object)t).GetPrimaryKeyValue()).Distinct().ToArray();
                    }
                    else
                    {
                        ids = deleteData.Select(t => ModelCoreHelper.GetPropertyValue(t, primaryKey)).Distinct().ToArray();
                    }
                    tsql.Append(ScriptHelper.GenerateDeleteScript(typeof(T), ids, schema, tableName, primaryKey, sufixParamName: "3"));
                }
                if (updateData != null && updateData.Count > 0)
                {
                    tsql.Append(ScriptHelper.GenerateScript(updateData, ModelState.Update, schema, tableName, primaryKey, null, sufixParamName: "1"));
                }
                if (insertData != null && insertData.Count > 0)
                {
                    tsql.Append(ScriptHelper.GenerateScript(updateData, ModelState.Insert, schema, tableName, primaryKey, null, sufixParamName: "2"));
                }
                tsql.Append(scriptCustomAfter);
                ExecuteNonQuery(CommandType.Text, cnn, transaction, tsql.Script, tsql.Param, commandTimeout);
            }
        }

        public void CloseConnection(IDbConnection cnn)
        {
            if (cnn != null)
            {
                cnn.Close();
            }
            cnn.Dispose();
        }

        public List<T> Query<T>(CommandType commandType, IDbConnection cnn, IDbTransaction transaction, ScriptHelperOutputBase param, int commandTimeout = DBConstant.CommonTimeoutNon)
        {
            List<T> result = new List<T>();
            if (param != null && !string.IsNullOrEmpty(param.Script))
            {
                result = Query<T>(commandType, cnn, transaction, param.Script, param.GetParam(), commandTimeout);
            }
            return result;
        }

        public virtual List<T> Query<T>(CommandType commandType, IDbConnection cnn, IDbTransaction transaction, string sql, object param, int commandTimeout = DBConstant.CommonTimeoutNon)
        {
            List<T> result = new List<T> { };
            if (!string.IsNullOrEmpty(sql))
            {
                if (IsSupportDBParam(param))
                {
                    IDbCommand cmd = CreateCommand(cnn, sql, param, commandType, commandTimeout);
                    result = (List<T>)Activator.CreateInstance<List<T>>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        DoQuery(typeof(T), result, reader);
                    }
                }
                else
                {
                    result = _PostgreSQLProvider.Query<T>(cnn, sql, param, commandType: commandType, transaction: transaction, commandTimeout: GetCommandTimeout(commandTimeout));
                }
            }
            return result;
        }

        public virtual bool CheckDuplicate<T>(T model) where T : BaseModel
        {
            Type type = model.GetType();
            PropertyInfo[] props = MemoryCacheService.GetPropertyInfo(type);
            ModelState modelState = (ModelState)model.GetValueByPropertyName(ColumnName.state);
            if (props != null)
            {
                PropertyInfo propertyInfoKey = props.SingleOrDefault(p => p.GetCustomAttribute<UniqueFieldAttribute>(true) != null);
                PropertyInfo primaryInfoKey = props.SingleOrDefault(p => p.GetCustomAttribute<KeyAttribute>(true) != null);
                if (propertyInfoKey != null)
                {
                    var tableAttr = model.GetTableAttribute();
                    var schema = tableAttr != null ? tableAttr.Schema : Constants.DefaultSchemaName;
                    var sql = $"select count(*) from {schema}.{type.Name} where {propertyInfoKey.Name} = '{model.GetValueByPropertyName(propertyInfoKey.Name)}' limit 1";
                    if(modelState == ModelState.Update && primaryInfoKey != null)
                    {
                        sql = $"select count(*) from {schema}.{type.Name} where {propertyInfoKey.Name} = '{model.GetValueByPropertyName(propertyInfoKey.Name)}' and {primaryInfoKey.Name} <> '{model.GetPrimaryKeyValue()}' limit 1";
                    }
                    List<int> data = QueryCommandTextOld<int>(DatabaseType.Business, DatabaseSide.ReadSide, sql);
                    if(data != null && data.Count > 0 && data[0] > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<T> GetAll<T>(DatabaseType dbType, string filter, string sort, string customFilter = "", string columns = "", string viewName = "", bool isEncoded = true) where T : BaseModel
        {
            string sql = "";
            string paging = "";

            if (isEncoded)
            {
                filter = Converter.DecodeBase64Param(filter);
                sort = Converter.DecodeBase64Param(sort);
                customFilter = Converter.DecodeBase64Param(customFilter);
            }

            WhereParameter whereFilter = GridFilterParser.Parse(filter);
            whereFilter = GridFilterParser.Parse(customFilter, whereFilter);

            var whereClause = whereFilter.GetWhereClause();
            if (!string.IsNullOrEmpty(whereClause))
            {
                whereClause = $" AND {whereClause}";
            }

            if (string.IsNullOrEmpty(columns))
            {
                sql = GenerateSelectAll<T>(viewName);
            }
            else
            {
                sql = GenerateSelectByColumn<T>(columns, viewName);
            }

            paging = GeneratePagingOld(1, Constants.MaxReturnRecord, sort);

            if (string.IsNullOrWhiteSpace(whereClause))
            {
                sql = $"{sql} {paging}";
            }
            else
            {
                sql = $"{sql} {whereClause} {paging}";
            }

            var list = QueryCommandTextOld<T>(dbType, DatabaseSide.ReadSide, sql, param: whereFilter.WhereValues);
            return list;
        }

        public virtual string GeneratePagingOld(int pageIndex, int pageSize, string sort, string selectedValue = null, bool isSort = true)
        {
            string sql = "";
            List<SelectedValue> selectedValues = null;
            if (!string.IsNullOrWhiteSpace(selectedValue))
            {
                selectedValues = Converter.Deserialize<List<SelectedValue>>(selectedValue);
            }
            if (!string.IsNullOrWhiteSpace(sort))
            {
                List<GridSortItem> sorts = Converter.Deserialize<List<GridSortItem>>(sort);

                if (sort?.Count() > 0)
                {
                    foreach (var sortItem in sorts)
                    {
                        if (SecureUtil.DetectSqlInjection(sortItem.property))
                        {
                            throw new FormatException();
                        }
                        sql += $"{sortItem.property} {(sortItem.desc ? "DESC" : "ASC")}, ";
                    }
                    if (selectedValue != null)
                    {
                        foreach (var valueItem in selectedValues)
                        {
                            if (SecureUtil.DetectSqlInjection(valueItem.property))
                            {
                                throw new FormatException();
                            }
                            sql = $"iif({valueItem.property} = '{valueItem.value}',0,1) ASC, " + sql;
                        }
                    }
                    sql = sql.Substring(0, sql.Length - 2);
                    sql = $" ORDER BY {sql}";
                }
                else
                {
                    if (isSort)
                    {
                        sql = $" ORDER BY created_date DESC";
                    }
                }
            }
            else if (selectedValues != null)
            {
                foreach (var valueItem in selectedValues)
                {
                    if (SecureUtil.DetectSqlInjection(valueItem.property))
                    {
                        throw new FormatException();
                    }
                    sql = $"iif({valueItem.property} = '{valueItem.value}',0,1) ASC, " + sql;
                }
                sql = sql.Substring(0, sql.Length - 2);
                sql = $" ORDER BY {sql}";
            }
            else
            {
                if (isSort)
                {
                    sql = $" ORDER BY created_date DESC";
                }
            }

            if (pageSize > 0)
            {
                sql += $" OFFSET {(pageIndex - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";
            }

            return sql;
        }

        public string GenerateSelectByColumn<T>(string columns, string viewName = "", string table = "", DatabaseType dbType = DatabaseType.Business) where T : BaseModel
        {
            string sql = "";
            viewName = SecureUtil.SafeSqlLiteral(viewName);
            table = SecureUtil.SafeSqlLiteral(table);
            var model = Activator.CreateInstance<T>();
            var tableAttr = model.GetTableAttribute();
            string schema = Constants.DefaultSchemaName;
            if (tableAttr != null && tableAttr.Schema != null)
            {
                schema = tableAttr.Schema;
            }

            if (string.IsNullOrEmpty(viewName))
            {
                viewName = ((ViewAttribute)model.GetType().GetCustomAttributes(typeof(ViewAttribute), false).FirstOrDefault())?.ViewName;
            }
            columns = SecureUtil.SafeSqlLiteral(columns);
            if (!string.IsNullOrEmpty(viewName))
            {
                sql = $"SELECT {columns} FROM {schema}.{viewName} WHERE 1=1";
            }
            else
            {
                string tableName = table;
                if (string.IsNullOrEmpty(table))
                {
                    tableName = tableAttr.Name;
                }
                sql = $"SELECT {columns} FROM {schema}.{tableName} WHERE 1=1";
            }

            return sql;
        }

        public List<T> QueryCommandTextOld<T>(DatabaseType databaseType, DatabaseSide dbSide, string sql, object param = null, int commandTimeout = -1)
        {
            List<T> result = new List<T>();
            var cnn = GetConnection();

            try
            {
                OpenConnection(cnn);
                var query = cnn.Query<T>(sql, param, commandType: CommandType.Text, commandTimeout: GetCommandTimeout(commandTimeout));
                result = query.ToList();
            }
            finally
            {
                CloseConnection(cnn);
            }
            return result;
        }

        public virtual string GenerateSelectAll<T>(string viewName = "", string table = "", DatabaseType dbType = DatabaseType.Business, bool isGetEditVersion = true) where T : BaseModel
        {
            string sql = "";
            Type modelType = null;
            BaseModel model = null;
            if (!string.IsNullOrEmpty(table))
            {
                modelType = GetModelType(table);
                model = (BaseModel)Activator.CreateInstance(modelType);
            }
            else
            {
                model = Activator.CreateInstance<T>();
            }

            var tableAttr = model.GetTableAttribute();
            viewName = SecureUtil.SafeSqlLiteral(viewName);
            table = SecureUtil.SafeSqlLiteral(table);
            string schemaName = tableAttr.Schema;
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = ((ViewAttribute)model.GetType().GetCustomAttributes(typeof(ViewAttribute), false).FirstOrDefault())?.ViewName;
            }

            if (!string.IsNullOrEmpty(viewName))
            {
                sql = $"SELECT * FROM {schemaName}.{viewName} WHERE 1=1";
            }
            else
            {
                string tableName = table;
                if (string.IsNullOrEmpty(table))
                {
                    tableName = tableAttr.Name;
                }
                if (model is ISubTable)
                {
                    sql = $"SELECT {((ISubTable)model).GetQueryColumn()} FROM {schemaName}.{viewName} WHERE 1=1";
                }
                else if (!isGetEditVersion)
                {
                    sql = $"SELECT * FROM {schemaName}.{tableName} WHERE 1=1";
                }
                else
                {
                    sql = $"SELECT *, {GetEditVersionColumn(schemaName + "." + tableName)} FROM {schemaName}.{tableName} WHERE 1=1";
                }
            }

            return sql;
        }
        
        public virtual Type GetModelType(string typeName)
        {
            Type type = null;
            lock (syncLockTypes)
            {
                if (_types.ContainsKey(typeName))
                {
                    type = _types[typeName];
                }
                else
                {
                    type = ModelHelper.GetType(typeName);
                    _types.Add(typeName, type);
                }
            }

            return type;
        }

        public virtual void Delete<T>(T model) where T : BaseModel
        {
            Type type = model.GetType();
            PropertyInfo[] props = MemoryCacheService.GetPropertyInfo(type);
            PropertyInfo propertyInfoKey = null;
            object key = "";
            if (props != null)
            {
                propertyInfoKey = props.SingleOrDefault(p => p.GetCustomAttribute<KeyAttribute>(true) != null);
                if (propertyInfoKey != null)
                {
                    var tableAttr = model.GetTableAttribute();
                    var schema = tableAttr != null ? tableAttr.Schema : Constants.DefaultSchemaName;
                    var sql = $"delete from {schema}.{type.Name} where {propertyInfoKey.Name} = '{model.GetPrimaryKeyValue()}'";
                    List<int> data = QueryCommandTextOld<int>(DatabaseType.Business, DatabaseSide.WriteSide, sql);
                }
            }
        }

        protected string GetEditVersionColumn(string tableName)
        {
            return $"{tableName}.xmin as edit_version";
        }

        private void DoQuery(Type type, IList result, IDataReader reader)
        {
            Dictionary<string, KeyValuePair<string, PropertyInfo>> columnMappings = null;
            while (reader.Read())
            {
                if (columnMappings == null)
                {
                    columnMappings = MappingObjectAndReader(type, reader);
                }
                if (columnMappings != null && columnMappings.Count > 0)
                {
                    var obj = Activator.CreateInstance(type);
                    MappingValueFromReader(obj, reader, columnMappings);
                    result.Add(obj);
                }
                else
                {
                    if (reader != null && reader.FieldCount > 0)
                    {
                        result.Add(reader[0]);
                    }
                }
            }
        }

        private bool IsSupportDBParam(object param)
        {
            if (param != null && (param is List<DBParam> || param is Dictionary<string, DBParam>))
                return true;
            else return false;
        }

        private IDbCommand CreateCommand(IDbConnection cnn, string sql, object param, CommandType commandType, int commandTimeout)
        {
            IDbCommand cmd = _PostgreSQLProvider.CreateCommand(cnn, sql, param, commandType, commandTimeout);
            if (param != null)
            {
                if (param is Dictionary<string, object>)
                {
                    if (!(cmd is NpgsqlCommand))
                    {
                        throw new Exception($"cnn ({cmd.GetType().Name}) <> NpgsqlCommand ==> not support");
                    }
                    foreach (var item in (Dictionary<string, object>)param)
                    {
                        ((NpgsqlCommand)cmd).Parameters.AddWithValue(item.Key, item.Value);
                    }
                }
                else if (param is Dictionary<string, DBParam>)
                {
                    InitCommandParam(((Dictionary<string, DBParam>)param).Values.ToList(), cmd);
                }
                else if (param is List<DBParam>)
                {
                    InitCommandParam(((List<DBParam>)param), cmd);
                }
                else
                {
                    throw new Exception($"IDBCommand CreateCommand - not support param type {param.GetType().Name}");
                }
            }
            cmd.CommandTimeout = GetCommandTimeout(commandTimeout);
            return cmd;
        }

        private static void InitCommandParam(List<DBParam> param, IDbCommand cmd)
        {
            if (param != null)
            {
                foreach (var item in param)
                {
                    if (item.DataTypeName == DBTypeName.none)
                    {
                        item.DataTypeName = DBTypeName.GetDBTypeName(item.Value);
                    }
                    if (item.Value == null)
                    {
                        cmd.Parameters.Add(new NpgsqlParameter { ParameterName = item.ParameterName, DataTypeName = item.DataTypeName, Value = DBNull.Value });
                    }
                    else
                    {
                        cmd.Parameters.Add(new NpgsqlParameter { ParameterName = item.ParameterName, DataTypeName = item.DataTypeName, Value = item.Value });
                    }
                }
            }
        }

        private int GetCommandTimeout(int commandTimeout)
        {
            return 90;
        }

        private void DoSaveData<T>(IDbConnection cnn, IDbTransaction transaction, List<T> lstModel, ModelState modelState, ScriptHelperOutputDBParam scriptCustomBefore, ScriptHelperOutputDBParam scriptCustomAfter, string schema, string tableName, string primaryKey, List<string> updateColumns, List<string> updateByColumnsNames, int commandTimeout)
        {
            switch (modelState)
            {
                case ModelState.None:
                    DoSaveDataNoneState<T>(cnn, transaction, lstModel, schema, tableName, primaryKey, updateByColumnsNames, scriptCustomBefore, scriptCustomAfter, commandTimeout);
                    break;
                default:
                    if (modelState == ModelState.Update && updateColumns != null && updateColumns.Count > 0 && updateByColumnsNames != null && updateByColumnsNames.Count > 0)
                    {
                        DoSaveDataByColumnName(cnn, transaction, lstModel, schema, tableName, updateColumns, updateByColumnsNames, scriptCustomBefore, scriptCustomAfter, commandTimeout);
                    }
                    else
                    {
                        DoSaveDataByState(cnn, transaction, lstModel, modelState, scriptCustomBefore, scriptCustomAfter, schema, tableName, primaryKey, commandTimeout);
                    }
                    break;
            }
        }

        private void DoSaveDataByColumnName<T>(IDbConnection cnn, IDbTransaction transaction, List<T> lstModel, string schema, string tableName, List<string> updateColumns, List<string> updateByColumnsNames, ScriptHelperOutputDBParam scriptCustomBefore, ScriptHelperOutputDBParam scriptCustomAfter, int commandTimeout)
        {
            if (lstModel != null && lstModel.Count > 0 && updateColumns != null && updateColumns.Count > 0 && updateByColumnsNames != null && updateByColumnsNames.Count > 0 && !string.IsNullOrEmpty(tableName))
            {
                ScriptHelperOutputSaveBatch tsql = new ScriptHelperOutputSaveBatch();
                tsql.Append(scriptCustomBefore);
                List<string> columnNames = new List<string>();
                columnNames.AddRange(updateColumns);
                columnNames.AddRange(updateByColumnsNames);
                ScriptHelperOutputSaveBatch scriptHelper = ScriptHelper.GetSaveBatchInput(lstModel, paramColumnNames: columnNames.Distinct().ToList());
                string scriptItem = $@"update {schema}.{tableName} a set {string.Join(",", updateColumns.Select(t => $"{t}=b.{t}"))} from ({scriptHelper.GetUnnestTableScript()}) b where {string.Join(" and ", updateByColumnsNames.Select(t => $"a.{t}=b.{t}"))} and {string.Join(" and ", updateColumns.Select(t => $"a.{t} is null or b.{t} is null or a.{t}<>b.{t})"))};";
                tsql.AppendScript(scriptItem);
                tsql.AppendParam(scriptHelper.Param);
                tsql.Append(scriptCustomAfter);
                ExecuteNonQuery(CommandType.Text, cnn, transaction, tsql.Script, tsql.Param, commandTimeout);
            }
        }

        private void DoSaveDataNoneState<T>(IDbConnection cnn, IDbTransaction transaction, List<T> lstModel, string schema, string tableName, string primaryKey, List<string> updateByColumnsNames, ScriptHelperOutputDBParam scriptCustomBefore, ScriptHelperOutputDBParam scriptCustomAfter, int commandTimeout)
        {
            if (lstModel != null && lstModel.Count > 0)
            {
                List<string> columnName = new List<string>() { primaryKey };
                if (updateByColumnsNames != null && updateByColumnsNames.Count > 0)
                {
                    columnName.AddRange(updateByColumnsNames);
                }
                else
                {
                    updateByColumnsNames = new List<string>() { primaryKey };
                }
                columnName = columnName.Distinct().ToList();
                ScriptHelperOutputSaveBatch queryOldDataScript = ScriptHelper.GetSaveBatchInput(lstModel, ModelState.Update, primaryKey, columnName);
                queryOldDataScript.Script = $"select a.* from {schema}.{tableName} a inner join ({queryOldDataScript.GetUnnestTableScript()}) b on ({string.Join(" and ", updateByColumnsNames.Select(t => $"a.{t}=n.{t}"))})";
                List<T> oldData = Query<T>(CommandType.Text, cnn, transaction, queryOldDataScript);
                Dictionary<string, T> dicOldData = new Dictionary<string, T>();
                if (oldData != null && oldData.Count > 0)
                {
                    foreach (T item in oldData)
                    {
                        string key = ModelCoreHelper.JoinColumnValues(item, updateByColumnsNames);
                        if (!dicOldData.ContainsKey(key))
                        {
                            dicOldData.Add(key, item);
                        }
                    }
                }
                List<T> updateData = new List<T>();
                Dictionary<string, T> insertData = new Dictionary<string, T>();
                foreach (T item in lstModel)
                {
                    string key = ModelCoreHelper.JoinColumnValues(item, updateByColumnsNames);
                    if (!dicOldData.ContainsKey(key))
                    {
                        if (!insertData.ContainsKey(key))
                        {
                            insertData.Add(key, item);
                        }
                    }
                    else
                    {
                        if (!insertData.ContainsKey(key))
                        {
                            ModelCoreHelper.SetPropertyValue(item, primaryKey, ModelCoreHelper.GetPropertyValue(dicOldData[key], primaryKey));
                            if (typeof(T) is ITrackingModel<T>)
                            {
                                ((ITrackingModel<T>)item).old_data = dicOldData[key];
                            }
                            updateData.Add(item);
                        }
                    }
                }
                SaveData(cnn, transaction, insertData.Values.ToList(), updateData, null, scriptCustomBefore, scriptCustomAfter, schema, tableName, primaryKey, commandTimeout);
            }
        }

        private void DoSaveDataByState<T>(IDbConnection cnn, IDbTransaction transaction, List<T> lstModel, ModelState modelState, ScriptHelperOutputDBParam scriptCustomBefore, ScriptHelperOutputDBParam scriptCustomAfter, string schema, string tableName, string primaryKey, int commandTimeout)
        {
            ScriptHelperOutputSaveBatch tsql = new ScriptHelperOutputSaveBatch();
            tsql.Append(scriptCustomBefore);
            tsql.Append(ScriptHelper.GenerateScript(lstModel, modelState, schema, tableName, primaryKey));
            tsql.Append(scriptCustomAfter);
            ExecuteNonQuery(CommandType.Text, cnn, transaction, tsql.Script, tsql.Param, commandTimeout);
        }

        /// <summary>
        /// Khởi tạo tham số cho command
        /// </summary>
        /// <param name="param"></param>
        /// <param name="cmd"></param>
        private static void InitCommandParam(List<DatabaseParamOld> param, NpgsqlCommand cmd)
        {
            if (param != null)
            {
                foreach (var item in param)
                {
                    if (item.Value == null)
                    {
                        cmd.Parameters.Add(new NpgsqlParameter { ParameterName = item.ParameterName, DataTypeName = item.DataTypeName, Value = DBNull.Value });
                    }
                    else
                    {
                        cmd.Parameters.Add(new NpgsqlParameter { ParameterName = item.ParameterName, DataTypeName = item.DataTypeName, Value = item.Value });
                    }
                }
            }
        }

        /// <summary>
        /// Mapping cấu trúc dữ liệu
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        private Dictionary<string, KeyValuePair<string, PropertyInfo>> MappingObjectAndReader(Type type, IDataReader dataReader)
        {
            Dictionary<string, KeyValuePair<string, PropertyInfo>> columnMappings = new Dictionary<string, KeyValuePair<string, PropertyInfo>>();
            foreach (PropertyInfo pi in type.GetProperties())
            {
                if (pi.CanWrite)
                {
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        if (pi.Name.Equals(dataReader.GetName(i), StringComparison.OrdinalIgnoreCase))
                        {
                            var propertyType = pi.PropertyType;

                            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                propertyType = propertyType.GetGenericArguments()[0];
                            }

                            columnMappings.Add(dataReader.GetName(i), new KeyValuePair<string, PropertyInfo>(propertyType.Name, pi));
                            break;
                        }
                    }
                }
            }

            return columnMappings;
        }

        /// <summary>
        /// Mapping dữ liệu
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="reader"></param>
        /// <param name=""></param>
        private void MappingValueFromReader(object temp, IDataReader reader, Dictionary<string, KeyValuePair<string, PropertyInfo>> columnMappings)
        {
            foreach (KeyValuePair<string, KeyValuePair<string, PropertyInfo>> columnMapping in columnMappings)
            {
                var value = reader[columnMapping.Key];
                if (value != null && value != DBNull.Value)
                {
                    var propertyTypeName = columnMapping.Value.Key;
                    var map = columnMapping.Value.Value;
                    switch (propertyTypeName)
                    {
                        case "Double":
                            double resDouble = 0;
                            if (double.TryParse(value.ToString(), out resDouble))
                            {
                                map.SetValue(temp, resDouble);
                            }
                            break;
                        case "Decimal":
                            decimal resDecimal = 0;
                            if (decimal.TryParse(value.ToString(), out resDecimal))
                            {
                                map.SetValue(temp, resDecimal);
                            }
                            break;
                        case "Int32":
                            int resInt32 = 0;
                            if (int.TryParse(value.ToString(), out resInt32))
                            {
                                map.SetValue(temp, resInt32);
                            }
                            break;
                        case "Int64":
                            long resInt64 = 0;
                            if (long.TryParse(value.ToString(), out resInt64))
                            {
                                map.SetValue(temp, resInt64);
                            }
                            break;
                        default:
                            map.SetValue(temp, value);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Lấy chuỗi kết nối
        /// </summary>
        /// <returns></returns>
        private string GetConnectionString()
        {
            // Build configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Access values using the configuration
            string host = configuration["ConnectionString:Host"] + "";
            string port = configuration["ConnectionString:Port"] + "";
            string username = configuration["ConnectionString:Username"] + "";
            string password = configuration["ConnectionString:Password"] + "";
            string database = configuration["ConnectionString:Database"] + "";

            return $"Host={host};Port={port};Username={username};Password={password};Database={database}";
        }
    }
}
