using System.Text;

namespace DemoWebAPI.Base.Model
{
    public class WhereParameter
    {
        private const string PrefixWhereParameter = " WHERE 1=1 ";
        private string _whereClause;
        private Dictionary<string, object> _whereValues = new Dictionary<string, object>();
        private bool _isAppendWhere = true;
        private List<object> _whereParam = new List<object>();

        public object WhereParam
        {
            get
            {
                return _whereParam;
            }
        }

        public string WhereClause
        {
            get
            {
                return _whereClause;
            }
        }
        public Dictionary<string, object> WhereValues
        {
            get
            {
                return _whereValues;
            }
        }
        public bool IsAppendWhere
        {
            get { return _isAppendWhere; }
        }

        public string CompiledWhereClause
        {
            get
            {
                return WhereParameter.FormatDynamicWhere(this);
            }
        }

        public void AddWhere(string sWhereClause, Dictionary<string, object> dictWhereValues, List<object> paramsDB = null)
        {
            StringBuilder stringBuilder = new StringBuilder();
            this._whereClause += stringBuilder.ToString();
            this._whereClause = $"({this._whereClause})";
            if(dictWhereValues != null && dictWhereValues.Count > 0)
            {
                foreach (KeyValuePair<string, object> current in dictWhereValues)
                {
                    string key = current.Key;
                    if(this._whereValues.ContainsKey(current.Key))
                    {
                        this._whereValues[key] = current.Value;
                    }
                    else
                    {
                        this._whereValues.Add(key, current.Value);
                    }
                }
            }
            if(paramsDB != null && paramsDB.Count > 0)
            {
                _whereParam.AddRange(paramsDB);
            }
        }

        public void AddWhere(WhereParameter oWhereParameter)
        {
            if(oWhereParameter != null)
            {
                this.AddWhere(oWhereParameter.WhereClause, oWhereParameter.WhereValues);
            }
        }

        public static string FormatDynamicWhere(WhereParameter whereParams)
        {
            if(whereParams != null && !string.IsNullOrWhiteSpace(whereParams.WhereClause))
            {
                StringBuilder stringBuilder = new StringBuilder(whereParams.WhereClause);
                foreach (KeyValuePair<string, object> current in whereParams._whereValues)
                {
                    KeyValuePair<string, object> keyValuePair = current;
                    if(current.Value != null)
                    {
                        if(current.Value.GetType() == typeof(string))
                        {
                            keyValuePair = new KeyValuePair<string, object>(current.Key, SecureUtil.SafeSqlLiteral(current.Value.ToString()));
                        }
                        stringBuilder.Replace("{" + keyValuePair.Key + "}", string.Concat(keyValuePair.Value) ?? "");
                    }
                }
                string result = string.Empty;
                if(whereParams.IsAppendWhere)
                {
                    result = WhereParameter.AppendWherePrefix(stringBuilder.ToString());
                }
                else
                {
                    result = stringBuilder.ToString();
                }
                return result;
            }
            return string.Empty;
        }

        public static string AppendWherePrefix(string whereClause)
        {
            if (!whereClause.Trim().StartsWith("WHERE ", StringComparison.OrdinalIgnoreCase))
            {
                if(!whereClause.Trim().StartsWith("AND ", StringComparison.OrdinalIgnoreCase))
                {
                    whereClause = "AND (" + whereClause + ")";
                }
                string prefixWhereParameter = PrefixWhereParameter;
                if(!string.IsNullOrEmpty(prefixWhereParameter))
                {
                    whereClause = prefixWhereParameter + whereClause;
                }
            }
            return whereClause;
        }

        public string GetWhereClause()
        {
            return this._whereClause;
        }
    }
}
