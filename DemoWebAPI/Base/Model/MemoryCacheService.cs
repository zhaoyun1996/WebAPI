using Npgsql.TypeMapping;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace DemoWebAPI.Base.Model
{
    public class MemoryCacheService
    {
        private static ConcurrentDictionary<Type, PropertyInfo[]> _cachePropertyInfo = new ConcurrentDictionary<Type, PropertyInfo[]>();
        private static ConcurrentDictionary<Type, List<PropertyInfo>> _cacheTablePropertyInfo = new ConcurrentDictionary<Type, List<PropertyInfo>>();

        public static PropertyInfo[] GetPropertyInfo(Type type)
        {
            if(!_cachePropertyInfo.ContainsKey(type))
            {
                var props = type.GetProperties().Where(p => p.GetIndexParameters().Length == 0).ToArray();
                _cachePropertyInfo.TryAdd(type, props);
            }
            return _cachePropertyInfo[type];
        }

        public static List<PropertyInfo> GetTablePropertyInfo(Type type)
        {
            if(!_cacheTablePropertyInfo.ContainsKey(type))
            {
                var props = type.GetProperties();
                var rs = new List<PropertyInfo>();
                foreach (var pr in props)
                {
                    var notMap = pr.GetCustomAttribute<NotMappedAttribute>();
                    if(notMap == null)
                    {
                        rs.Add(pr);
                    }
                }

                _cacheTablePropertyInfo.TryAdd(type, rs);
            }

            return _cacheTablePropertyInfo[type];
        }
    }
}
