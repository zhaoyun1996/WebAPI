using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Text.Json.Serialization;

namespace DemoWebAPI.Base.Model
{
    public class GridFilterParser
    {
        public static WhereParameter Parse(string input, WhereParameter where = null)
        {
            WhereParameter result = new WhereParameter();
            if (!string.IsNullOrEmpty(input))
            {
                var config = Converter.GetJsonSerializerSettings();
                JArray obj = JsonConvert.DeserializeObject<JArray>(input, config);

                StringBuilder builder = new StringBuilder();
                Dictionary<string, object> parameters = new Dictionary<string, object>();

                if (where != null)
                {
                    parameters = where.WhereValues;
                    if (!string.IsNullOrWhiteSpace(where.GetWhereClause()))
                    {
                        builder.Append(" ( ");
                        builder.Append(where.GetWhereClause());
                        builder.Append(" ) ");
                    }
                    if (obj.Count > 0 && (builder.Length > 0 || !string.IsNullOrWhiteSpace(builder.ToString())))
                    {
                        builder.Append(" AND ");
                    }
                    builder.Append(" ( ");
                }

                ConvertArray2(obj, builder, parameters);

                if (where != null)
                {
                    builder.Append(")");
                }

                result.AddWhere(builder.ToString(), parameters);
            }
            else if (where != null)
            {
                result = where;
            }

            return result;
        }

        private static void ConvertArray2(JArray arr, StringBuilder builder, Dictionary<string, object> parameters)
        {
            if(arr.First != null && arr.First.Type == JTokenType.Array)
            {
                foreach (JToken item in arr)
                {
                    if(item.Type == JTokenType.Array)
                    {
                        builder.Append(" (");
                        ConvertArray2((JArray)item, builder, parameters);
                        builder.Append(") ");
                    }
                    else
                    {
                        if(string.Compare(item.Value<string>(), "and", true) == 0)
                        {
                            builder.Append(" AND ");
                        }
                        else if(string.Compare(item.Value<string>(), "or", true) == 0)
                        {
                            builder.Append(" OR ");
                        }
                    }
                }
            }
            else
            {
                builder.Append(ConvertFilterItem2((JArray)arr, parameters));
            }
        }

        private static string ConvertFilterItem2(JArray item, Dictionary<string, object> parameters, bool caseSensitive = false)
        {
            string column = item.First.Value<string>();
            string operatorValue = item.First.Next.Value<string>();
            object paramValue = null;
            string paramName = "p" + (parameters.Count + 1).ToString();
            string paramNameAlias = $":{paramName}";
            string operatorAlias = operatorValue;
            string pattern = " {0} {1} {2}";
            object dicValue;
            string stringValue;
            var valueItem = item.Count >= 3 ? item[2] : item.Last;

            switch (valueItem.Type)
            {
                case JTokenType.TimeSpan:
                    dicValue = valueItem.Value<TimeSpan>();
                    break;
                case JTokenType.Date:
                    column += "::date";
                    dicValue = valueItem.Value<DateTime>();
                    break;
                case JTokenType.Integer:
                    dicValue = valueItem.Value<Int64>();
                    break;
                case JTokenType.Float:
                    dicValue = valueItem.Value<decimal>();
                    break;
                case JTokenType.Boolean:
                    dicValue = valueItem.Value<bool>();
                    break;
                default:
                    dicValue = stringValue = valueItem.Value<string>().ToString();
                    Guid guid = Guid.Empty;
                    if(Guid.TryParse(stringValue, out guid))
                    {
                        dicValue = guid;
                    }
                    break;
            }

            switch (operatorValue.ToLower())
            {
                case "like":
                    operatorAlias = " like ";
                    dicValue = $"%{ProcessLikeValue(dicValue as string)}%";
                    break;
                case "is":
                    operatorAlias = " = ";
                    break;
                case "contains":
                    operatorAlias = " ILIKE ";
                    dicValue = $"%{ProcessLikeValue(dicValue as string)}%";
                    break;
                case "notcontains":
                    operatorAlias = " NOT ILIKE ";
                    dicValue = $"%{ProcessLikeValue(dicValue as string)}%";
                    if((dicValue as string).Length > 0)
                    {
                        pattern = $"({{0}} IS NULL OR {pattern})";
                    }
                    break;
                case "startswith":
                    operatorAlias = " ILIKE ";
                    dicValue = $"%{ProcessLikeValue(dicValue as string)}%";
                    break;
                case "notstartswith":
                    operatorAlias = " NOT ILIKE ";
                    dicValue = $"%{ProcessLikeValue(dicValue as string)}%";
                    if ((dicValue as string).Length > 0)
                    {
                        pattern = $"({0} IS NULL OR {pattern})";
                    }
                    break;
                case "endswith":
                    operatorAlias = " ILIKE ";
                    dicValue = $"%{ProcessLikeValue(dicValue as string)}%";
                    break;
                case "in":
                    var temp = dicValue.ToString().Split(",");
                    var ch = '\'';
                    Guid gValue;
                    decimal dValue;
                    List<object> values = new List<object>();
                    bool flag = true;
                    foreach (string tem in temp)
                    {
                        if (!string.IsNullOrEmpty(tem))
                        {
                            if (tem[0] == ch && tem[tem.Length - 1] == ch && Guid.TryParse(tem.Substring(1, tem.Length - 2), out gValue)){
                                values.Add(gValue);
                            }
                            else if(Guid.TryParse(tem, out gValue))
                            {
                                values.Add(gValue);
                            }
                            else if(decimal.TryParse(tem, out dValue))
                            {
                                flag = false;
                                break;
                            }
                        }
                    }

                    if(flag)
                    {
                        var inClause = new List<string>();
                        int iCount = parameters.Count;
                        foreach (var v in values)
                        {
                            paramName = $"p{++iCount}";
                            parameters.Add(paramName, v);
                            inClause.Add($"{column}=:{paramName}");
                        }
                        return $"({string.Join(" OR ", inClause)})";
                    }
                    else
                    {
                        var inClause = new List<string>();
                        int iCount = parameters.Count;
                        for (int i = 0; i < temp.Count(); ++i)
                        {
                            temp[i] = temp[i].Replace("'", "");
                            if (temp[i].Contains("&#44"))
                            {
                                temp[i] = temp[i].Replace("&#44", ",");
                            }
                            if(!caseSensitive)
                            {
                                paramName = $"p{++iCount}";
                                parameters.Add(paramName, temp[i]);
                                inClause.Add($"{column} ILIKE :{paramName}");
                            }
                        }
                        if (!caseSensitive)
                        {
                            return $"({string.Join(" OR ", inClause)})";
                        }
                        else
                        {
                            dicValue = temp;
                            pattern = "{0} = ANY(Array[{2}])";
                        }
                    }
                    break;
                case "notin":
                    var tempNotIn = dicValue.ToString().Split(",");
                    var chNotIn = '\'';
                    Guid gValueNotIn;
                    decimal dValueNotIn;
                    List<object> valuesNotIn = new List<object>();
                    bool flagNotIn = true;
                    foreach (string tem in tempNotIn)
                    {
                        if (!string.IsNullOrEmpty(tem))
                        {
                            if (tem[0] == chNotIn && tem[tem.Length - 1] == chNotIn && Guid.TryParse(tem.Substring(1, tem.Length - 2), out gValueNotIn))
                            {
                                valuesNotIn.Add(gValueNotIn);
                            }
                            else if(Guid.TryParse(tem, out gValueNotIn))
                            {
                                valuesNotIn.Add(gValueNotIn);
                            }
                            else if(decimal.TryParse(tem, out dValueNotIn))
                            {
                                valuesNotIn.Add(dValueNotIn);
                            }
                            else
                            {
                                flagNotIn = false;
                                break;
                            }
                        }
                    }

                    if(flagNotIn)
                    {
                        var notInClause = new List<string>();
                        int iCount = parameters.Count;
                        foreach (var v in valuesNotIn)
                        {
                            paramName = $"p{++iCount}";
                            parameters.Add(paramName, v);
                            notInClause.Add($"{column}=:{paramName}");
                        }

                        return $"({string.Join(" AND ", notInClause)})";
                    }
                    else
                    {
                        dicValue = tempNotIn;
                        pattern = "{0} <> ALL(Array[{2}]) OR {0} IS NULL";
                    }
                    break;
                case "is null":
                    operatorAlias = " IS NULL ";
                    if(valueItem.Value<string>() == "text")
                    {
                        pattern = "({0} IS NULL OR {0} =:" + paramName + ")";
                        dicValue = "";
                    }
                    else
                    {
                        pattern = "{0} IS NULL";
                    }
                    break;
                case "is not null":
                    operatorAlias = " IS NOT NULL ";
                    if (valueItem.Value<string>() == "text")
                    {
                        pattern = "({0} IS NOT NULL OR {0} =:" + paramName + ")";
                        dicValue = "";
                    }
                    else
                    {
                        pattern = "{0} IS NOT NULL";
                    }
                    break;
                case "=":
                case ">=":
                case "<=":
                    if(item.Count > 3 && (0.Equals(dicValue) || false.Equals(dicValue)))
                    {
                        var nullToFail = item[3].Value<bool>();
                        if(nullToFail)
                        {
                            pattern = $"({{0}} is null or {pattern})";
                        }
                    }
                    break;
                case "!=":
                    if (item.Count > 3 && (0.Equals(dicValue) || false.Equals(dicValue)))
                    {

                    }
                    else
                    {
                       pattern = $"({{0}} is null or {pattern})";
                    }
                    break;
                default:
                    break;
            }

            parameters.Add(paramName, paramValue);
            string res = string.Format(pattern, column, operatorAlias, paramNameAlias);
            return res;
        }

        private static string ProcessLikeValue(string value)
        {
            return value.Replace(@"\", @"\\").Replace("%", "\\%");
        }
    }
}
