using Microsoft.OpenApi.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using static DemoWebAPI.Constant.Enum;

namespace DemoWebAPI.Base.Model
{
    public class ScriptHelperOutputBase
    {
        public string Script { get; set; }

        /// <summary>
        /// Danh sách loại dữ liệu trả ra (dùng chung cho việc thực thi câu lệnh trả ra nhiều bảng kết quả)
        /// </summary>
        public Dictionary<string, Type> Types { get; set; }

        public ScriptHelperOutputBase() { }

        public ScriptHelperOutputBase(string script)
        {
            this.AppendScript(script);
        }

        public virtual object GetParam()
        {
            return null;
        }

        /// <summary>
        /// Gộp 2 đối tượng output vào nhau
        /// </summary>
        /// <param name="scriptHelperOutput"></param>
        /// <param name="endScript"></param>
        /// <returns></returns>
        protected ScriptHelperOutputBase Append(ScriptHelperOutputBase scriptHelperOutput, string endScript = "")
        {
            if (scriptHelperOutput != null)
            {
                AppendScript(scriptHelperOutput.Script, endScript);
            }

            return this;
        }

        /// <summary>
        /// Gộp script
        /// </summary>
        /// <param name="script"></param>
        /// <param name="endScript"></param>
        public void AppendScript(string script, string endScript = "")
        {
            if (!string.IsNullOrEmpty(script) && ValidateScript(script))
            {
                if (!string.IsNullOrEmpty(this.Script))
                {
                    this.Script = $"{this.Script}{script}";
                }
                else
                {
                    this.Script = script;
                }
                this.Script = this.Script.Trim();
                if (!string.IsNullOrEmpty(endScript) && !this.Script.EndsWith(endScript))
                {
                    this.Script = $"{this.Script}{endScript}";
                }
            }
        }

        /// <summary>
        /// Validate tham số trước khi sử dụng
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected bool CheckSupportParamType(object param)
        {
            if (param != null && param is Array)
            {
                throw new Exception("Không hỗ trợ định dạng này");
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Validate script để đảm bảo lúc thực thi không bị injection
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected bool ValidateScript(string sql)
        {
            if (!string.IsNullOrEmpty(sql) && sql.Contains("'"))
            {
                throw new Exception("Không cho phép chưa ký tự <'>");
            }
            else
            {
                return true;
            }
        }
    }

    /// <summary>
    /// Output của các hàm gen script
    /// </summary>
    public class ScriptHelperOutput : ScriptHelperOutputBase
    {
        public Dictionary<string, object> Param { get; set; } = new Dictionary<string, object>();

        public ScriptHelperOutput() { }

        public ScriptHelperOutput(string script) : base(script) { }

        public ScriptHelperOutput(string script, Dictionary<string, object> param) : base(script)
        {
            this.AppendParam(param);
        }

        public override object GetParam()
        {
            return this.Param;
        }

        public ScriptHelperOutput Append(ScriptHelperOutput scriptHelperOutput, string endScript = "")
        {
            if (scriptHelperOutput != null)
            {
                AppendParam(scriptHelperOutput.Param);
            }
            base.Append(scriptHelperOutput, endScript);
            return this;
        }

        /// <summary>
        /// Gộp tham số
        /// </summary>
        /// <param name="param"></param>
        public void AppendParam(Dictionary<string, object> param)
        {
            if (param != null && param.Count > 0)
            {
                if (this.Param != null || this.Param.Count == 0)
                {
                    this.Param = param;
                }
                else
                {
                    foreach (KeyValuePair<string, object> item in param)
                    {
                        AppendParam(item.Key, item.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Gộp tham số
        /// </summary>
        /// <param name="param"></param>
        public void AppendParam(string key, object value)
        {
            if (!this.Param.ContainsKey(key))
            {
                if (CheckSupportParamType(value))
                {
                    this.Param.Add(key, value);
                }
            }
            else if (this.Param[key] != null && value != null && this.Param[key].ToString() != value.ToString())
            {
                throw new Exception($"tham số {key} không thể nhận nhiều giá trị khác nhau");
            }
        }
    }

    public class ScriptHelperOutputSaveBatch : ScriptHelperOutputDBParam
    {
        public Dictionary<string, PropertyInfo> FieldBuildScripts { get; set; } = new Dictionary<string, PropertyInfo>();

        public Dictionary<string, PropertyInfo> FieldNotNulls { get; set; } = new Dictionary<string, PropertyInfo>();

        public ScriptHelperOutputSaveBatch() { }

        public ScriptHelperOutputSaveBatch(string script, List<DBParam> param)
        {
            this.AppendScript(script);
            this.AppendParam(param);
        }

        public void AppendFieldNotNulls(string columnName, PropertyInfo key)
        {
            if (!this.FieldNotNulls.ContainsKey(columnName))
            {
                this.FieldNotNulls.Add(columnName, key);
            }
        }

        public void AppendFieldBuildScripts(string columnName, PropertyInfo key)
        {
            if (!this.FieldNotNulls.ContainsKey(columnName))
            {
                this.FieldNotNulls.Add(columnName, key);
            }
        }

        public void AppendParam(Dictionary<PropertyInfo, List<object>> dataParam, string sufixParamName = "")
        {
            foreach (KeyValuePair<PropertyInfo, List<object>> item in dataParam)
            {
                if (this.FieldNotNulls != null && this.FieldNotNulls.ContainsKey($"{item.Key.Name}{sufixParamName}"))
                {
                    this.AppendParam($"{item.Key.Name}{sufixParamName}", $"{DBTypeName.GetDBTypeName(item.Key.PropertyType)}[]", item.Value.ToArray());
                }
            }
        }

        public string GetUnnestTableScript()
        {
            List<string> fieldNotNulls = this.FieldNotNulls.Keys.ToList();
            return GetUnnestTableScript(fieldNotNulls);
        }

        public string GetUnnestTableScript(List<string> arrayParameterNames)
        {
            return $"select {string.Join($",{Environment.NewLine} ", arrayParameterNames)} from unnest ({string.Join($",{Environment.NewLine} ", arrayParameterNames.Select(t => $":{t}"))}) as b({string.Join($",{Environment.NewLine} ", arrayParameterNames)})";
        }

        public Dictionary<string, string> GetFieldAndParam(ModelState modelState, string primaryKey = "", string sufixParamName = "", string before = "@", string after = "")
        {
            Dictionary<string, string> sField = new Dictionary<string, string>();
            string[] notUpdatField = { $"{primaryKey}{sufixParamName}", $"{ColumnNameCore.created_by}{sufixParamName}", $"{ColumnNameCore.created_date}{sufixParamName}", };
            foreach (var item in this.FieldBuildScripts.Where(t => !notUpdatField.Contains(t.Key) || modelState != ModelState.Update))
            {
                var prop = item.Value;
                string sFieldValue = item.Value.Name;
                string sParamName = item.Key;
                string sParamValue;
                if (this.FieldNotNulls.ContainsKey(sParamName))
                {
                    if (ModelCoreHelper.IsDBJsonField(prop))
                    {
                        sParamName = $"CAST({before}{sParamName}{after} as json)";
                    }
                    else if(ModelCoreHelper.IsDBTSvectorField(prop))
                    {
                        sParamValue = $"to_tsvector({before}{sParamName}{after})";
                    }
                    else
                    {
                        sParamValue = $"{before}{sParamName}{after}";
                    }
                }
                else
                {
                    sParamValue = "null";
                }
                sField.Add(sParamName, sFieldValue);
            }

            return sField;
        }
    }

    public class ScriptHelperOutputDBParam : ScriptHelperOutputBase
    {
        public Dictionary<string, DBParam> Param { get; set; } = new Dictionary<string, DBParam>();

        public ScriptHelperOutputDBParam() { }

        public ScriptHelperOutputDBParam(string script) : base(script) { }

        public ScriptHelperOutputDBParam(string script, List<DBParam> param) : base(script)
        {
            this.AppendParam(param);
        }

        public override object GetParam()
        {
            return this.Param;
        }

        public ScriptHelperOutputDBParam Append(ScriptHelperOutputDBParam scriptHelperOutput, string endScript = "")
        {
            base.Append(scriptHelperOutput, endScript);
            if (scriptHelperOutput != null)
            {
                AppendParam(scriptHelperOutput.Param);
            }
            return this;
        }

        public void AppendType(string key, Type type)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = type.Name;
            }
            if (this.Types == null)
            {
                this.Types = new Dictionary<string, Type>();
            }
            if (!this.Types.ContainsKey(key))
            {
                this.Types.Add(key, type);
            }
            else
            {
                throw new Exception($"AppendType - đã tồn tại key {key}");
            }
        }

        public void AppendParam(Dictionary<string, DBParam> paramMISASupport)
        {
            if (paramMISASupport != null && paramMISASupport.Count > 0)
            {
                if (this.Param != null && this.Param.Count == 0)
                {
                    this.Param = paramMISASupport;
                }
                else
                {
                    foreach (DBParam item in paramMISASupport.Values)
                    {
                        AppendParam(item);
                    }
                }
            }
        }

        public void AppendParam(Dictionary<string, object> param)
        {
            if (param != null && param.Count > 0)
            {
                foreach (KeyValuePair<string, object> item in param)
                {
                    AppendParam(item.Key, DBDataType.none, item.Value);
                }
            }
        }

        public void AppendParam(List<DBParam> paramMISASupport)
        {
            if (paramMISASupport != null && paramMISASupport.Count > 0)
            {
                foreach (DBParam item in paramMISASupport)
                {
                    AppendParam(item);
                }
            }
        }

        public void AppendParam(string parameterName, string dataTypeName, object value)
        {
            if (dataTypeName != DBTypeName.none)
            {
                dataTypeName = DBTypeName.GetDBTypeName(value);
            }
            AppendParam(new DBParam(parameterName, dataTypeName, value));
        }

        public void AppendParam(string parameterName, DBDataType dbDataType, object value)
        {
            string dataTypeName = dbDataType.ToString();
            if (dataTypeName != DBTypeName.none)
            {
                dataTypeName = DBTypeName.GetDBTypeName(value);
                AppendParam(new DBParam(parameterName, dataTypeName, value));
            }
            else
            {
                AppendParam(new DBParam(parameterName, dbDataType, value));
            }
        }

        public void AppendParam(DBParam param)
        {
            if (!this.Param.ContainsKey(param.ParameterName))
            {
                this.Param.Add(param.ParameterName, param);
            }
            else if (this.Param[param.ParameterName] != null && param.Value != null && this.Param[param.ParameterName] != null && this.Param[param.ParameterName].Value.ToString() != param.Value.ToString())
            {
                throw new Exception($"Tham số {param.DataTypeName} không thể nhận cùng lúc 2 giá trị khác nhau {this.Param[param.DataTypeName].Value} <> {param.Value}");
            }
        }
    }
}
