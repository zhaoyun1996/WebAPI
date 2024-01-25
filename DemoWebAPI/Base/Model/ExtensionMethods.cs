namespace DemoWebAPI.Base.Model
{
    public static class ExtensionMethods
    {
        public static object GetValueByPropertyName(this object data, string propertyName)
        {
            var pr = data.GetType().GetProperty(propertyName);
            return pr != null ? pr.GetValue(data) : null;
        }

        public static void SetValueByPropertyName(this object data, string propertyName, object value)
        {
            var pr = data.GetType().GetProperty(propertyName);
            if(pr != null)
            {
                pr.SetValue(data, value, null);
            }
        }
    }
}
