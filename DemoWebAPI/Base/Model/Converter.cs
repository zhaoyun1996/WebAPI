using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Text.Json;

namespace DemoWebAPI.Base.Model
{
    public class Converter
    {
        private static JsonSerializerSettings _jsonSerializerSettings;
        private static readonly DateFormatHandling JSONDateFormatHandling = DateFormatHandling.IsoDateFormat;
        private static readonly DateTimeZoneHandling JSONDateTimeZoneHandling = DateTimeZoneHandling.Local;
        private static readonly string JSONDateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffFFFFK'";
        private static readonly NullValueHandling JSONNulValueHandling = NullValueHandling.Ignore;
        private static readonly ReferenceLoopHandling JSONReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        public static string Base64Encode(string clearText)
        {
            if(string.IsNullOrWhiteSpace(clearText))
            {
                return string.Empty;
            }
            return Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(clearText));
        }

        public static string Base64Decode(string base64Text)
        {
            if (string.IsNullOrWhiteSpace(base64Text))
            {
                return string.Empty;
            }
            return UTF8Encoding.UTF8.GetString(Convert.FromBase64String(base64Text));
        }

        public static string DecodeBase64Param(string filter)
        {
            if(string.IsNullOrEmpty(filter))
            {
                return string.Empty;
            }
            filter = Base64Decode(filter);
            filter = UrlDecode(filter).Replace("\\", "");
            return filter;
        }

        public static string UrlDecode(string url)
        {
            return WebUtility.UrlDecode(url);
        }

        public static JsonSerializerSettings GetJsonSerializerSettings()
        {
            if(_jsonSerializerSettings == null)
            {
                _jsonSerializerSettings = new JsonSerializerSettings()
                {
                    DateFormatHandling = JSONDateFormatHandling,
                    DateTimeZoneHandling = JSONDateTimeZoneHandling,
                    DateFormatString = JSONDateFormatString,
                    NullValueHandling = JSONNulValueHandling,
                    ReferenceLoopHandling = JSONReferenceLoopHandling
                };
            }
            return _jsonSerializerSettings;
        }

        public static T Deserialize<T>(string json, JsonSerializerSettings settings = null)
        {
            if (settings == null)
            {
                settings = GetJsonSerializerSettings();
            }
            return JsonConvert.DeserializeObject<T>(json, settings);
        }
    }
}
