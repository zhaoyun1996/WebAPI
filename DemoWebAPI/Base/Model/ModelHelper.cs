using System.Collections.Concurrent;

namespace DemoWebAPI.Base.Model
{
    public class ModelHelper
    {
        private static ConcurrentDictionary<string, Type> _modelType = new ConcurrentDictionary<string, Type>();
        public static Type GetType(string modelName)
        {
            if(!_modelType.ContainsKey(modelName))
            {
                _modelType.TryAdd(modelName, Type.GetType("DemoWebAPI.Model." + modelName));
            }

            return _modelType[modelName];
        }
    }
}
