using System.Text.Json.Serialization;

namespace DemoWebAPI.Core.Model
{
    public class SessionData : SessionDataCache
    {
        public string SessionId { get; set; }
    }

    public class ContextData
    {
        public string SessionId { get; set; }
    }

    [Serializable]
    public class UserInfo
    {
        public string UserName { get; set; }
    }

    [Serializable]
    public class SessionDataCache : ICloneable
    {
        public UserInfo User { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
