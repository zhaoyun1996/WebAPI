﻿using DemoWebAPI.Base.Interface;
using DemoWebAPI.Base.Model;
using DemoWebAPI.Model;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Serialization;
using Npgsql;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.Base.DL
{
    public class DLBase
    {
        /// <summary>
        /// Thực thi lệnh tương tác với database, trả về kết quả 1 bảng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public List<T> ExecuteData<T>(string sql, List<DatabaseParamOld> param, CommandType commandType = CommandType.Text, int commandTimeout = 30)
        {
            List<T> result = Activator.CreateInstance<List<T>>();

            //try
            //{
            //    IDbConnection cnn = this.GetConnection();
            //    OpenConnection(cnn);

            //    var cmd = cnn.CreateCommand();
            //    cmd.CommandType = CommandType.Text;
            //    cmd.CommandText = sql;
            //    cmd.commandTimeout = commandTimeout;
            //    InitCommandParam(param, cmd);

            //    using (var reader = cmd.ExecuteReader())
            //    {
            //        Dictionary<string, KeyValuePair<string, PropertyInfo>> columnMappings = null;
            //        Type type = typeof(T);
            //        while (reader.Read())
            //        {
            //            if (columnMappings == null)
            //            {
            //                columnMappings = MappingObjectAndReader(type, reader);
            //            }
            //            T obj = Activator.CreateInstance<T>();
            //            MappingValueFromReader(obj, reader, columnMappings);
            //            result.Add(obj);
            //        }
            //    }
            //}
            //catch (Exception)
            //{

            //    throw;
            //}
            //finally
            //{
            //    if (cnn != null)
            //    {
            //        if (cnn.State != ConnectionState.Closed)
            //        {
            //            cnn.Close();
            //        }

            //        cnn.Dispose();
            //    }
            //}

            return result;
        }

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
                    result = _PostgreSQLProvider.Execute(cnn, sql, param, transaction, GetcommandTimeout(commandTimeout), commandType);
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
                if(insertData != null && insertData.Count > 0)
                {
                    tsql.Append(ScriptHelper.GenerateScript(updateData, ModelState.Insert, schema, tableName, primaryKey, null, sufixParamName: "2"));
                }
                tsql.Append(scriptCustomAfter);
                ExecuteNonQuery(CommandType.Text, cnn, transaction, tsql.Script, tsql.Param, commandTimeout);
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
            cmd.CommandTimeout = GetcommandTimeout(commandTimeout);
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

        private int GetcommandTimeout(int commandTimeout)
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
                    if (modelState == ModelState.Update)
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

        private void DoSaveDataByState<T>(IDbConnection cnn, IDbTransaction tran, List<T> lstModel, ModelState modelState, ScriptHelperOutputDBParam scriptCustomBefore, ScriptHelperOutputDBParam scriptCustomAfter, string schema, string tableName, string primaryKey, int commandTimeout)
        {

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
