using DemoWebAPI.Model;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
            List<T> result = Activator.CreateInstance<List<T>>()   

            try
            {
                IDbConnection cnn = this.GetConnection();
                OpenConnection(cnn);

                var cmd = cnn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.CommandTimeout = commandTimeout;
                InitCommandParam(param, cmd);

                using (var reader = cmd.ExecuteReader())
                {
                    Dictionary<string, KeyValuePair<string, PropertyInfo>> columnMappings = null;
                    Type type = typeof(T);
                    while (reader.Read())
                    {
                        if (columnMappings == null)
                        {
                            columnMappings = MappingObjectAndReader(type, reader);
                        }
                        T obj = Activator.CreateInstance<T>();
                        MappingValueFromReader(obj, reader, columnMappings);
                        result.Add(obj);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (cnn != null)
                {
                    if (cnn.State != ConnectionState.Closed)
                    {
                        cnn.Close();
                    }

                    cnn.Dispose();
                }
            }

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
            if(cnn.State != ConnectionState.Open)
            {
                cnn.Open();
            }
        }

        /// <summary>
        /// Thực hiện thêm dữ liệu
        /// </summary>
        /// <returns></returns>
        public Dictionary<object, Exception> InsertData<T>(IDbConnection cnn, IDbTransaction transaction, List<T> lstModel, ModelState modelState,  string schema = "", string tableName = "", string primaryKey = "", int commandTimeOut = 30)
        {
            Dictionary<object, Exception> exceptionData = new Dictionary<object, Exception>();

            try
            {
                if(lstModel != null && lstModel.Count > 0)
                {
                    int page = 0;
                    int pageSize = 10000;

                    while(page * pageSize <= lstModel.Count)
                    {
                        var pagingData = lstModel.Skip(page* pageSize).Take(pageSize).ToList();

                        if(pagingData.Count > 0)
                        {
                            IDbTransaction tran = transaction;

                            try
                            {
                                if(page == 0)
                                {
                                    DoSaveData(cnn, tran, pagingData, modelState, schema, tableName, primaryKey, commandTimeOut);
                                }
                            }
                            catch (Exception)
                            {

                                throw;
                            }
                            finally
                            {
                                if (tran != null && transaction == null)
                                {
                                    tran.Dispose();
                                    tran = null;
                                }
                            }
                        }

                        page += 1;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return exceptionData;
        }

        private void DoSaveData<T>(IDbConnection cnn, IDbTransaction tran, List<T> lstModel, ModelState modelState, string schema, string tableName, string primaryKey, int commandTimeOut)
        {
            switch (modelState)
            {
                case ModelState.None:
                    //DoSaveDataNoneState<T>(cnn, tran, lstModel, modelState, schema, tableName, primaryKey, commandTimeOut);
                    break;
                default:
                    if(modelState == ModelState.Update)
                    {
                        
                    }
                    else
                    {
                        DoSaveDataByState(cnn, tran, lstModel, modelState, schema, tableName, primaryKey, commandTimeOut);
                    }
                    break;
            }
        }

        private void DoSaveDataByState(IDbConnection cnn, IDbTransaction tran, List<T> lstModel, ModelState modelState, string schema, string tableName, string primaryKey, int commandTimeOut)
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
